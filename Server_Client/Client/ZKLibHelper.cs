using System;
using System.Collections.Generic;
using System.Linq;

namespace Client
{
    public class ZKLibHelper
    {
        public const int USHRT_MAX = 65535;
        public static byte[] CreateTCPHeader(COMMANDS command, int? sessionId, int? replyId, byte[] data)
        {


            var xx = CreateHeader((int)command, 0, sessionId ?? 0, replyId ?? 0, data);
            Console.WriteLine(command.ToString() + ":" + ((int)command));
            Console.WriteLine(string.Join(", ", xx));
            Console.WriteLine("=================================================================");
            return xx;
            
        }


        private static byte[] CreateHeader(int command, int chksum, int sessionId, int replyId, byte[] data)
        {
            byte[] dataBuffer = data ?? System.Text.Encoding.UTF8.GetBytes(string.Empty);
            byte[] buf = new byte[8 + dataBuffer.Length];
            byte[] commandBytes = BitConverter.GetBytes((ushort)command);
            byte[] chksumBytes = BitConverter.GetBytes((ushort)chksum);
            byte[] sessionIdBytes = BitConverter.GetBytes((ushort)sessionId);
            byte[] replyIdBytes = BitConverter.GetBytes((ushort)replyId);

            Buffer.BlockCopy(commandBytes, 0, buf, 0, 2);
            Buffer.BlockCopy(chksumBytes, 0, buf, 2, 2);
            Buffer.BlockCopy(sessionIdBytes, 0, buf, 4, 2);
            Buffer.BlockCopy(replyIdBytes, 0, buf, 6, 2);

            byte[] commandStringBytes = dataBuffer;
            Buffer.BlockCopy(commandStringBytes, 0, buf, 8, commandStringBytes.Length);

            byte[] bufChksum = CheckSum(buf);
            Buffer.BlockCopy(bufChksum, 0, buf, 2, 2);


            {
                replyId = (replyId + 1) % USHRT_MAX;
                replyIdBytes = BitConverter.GetBytes((ushort)replyId);
                Buffer.BlockCopy(replyIdBytes, 0, buf, 6, 2);
            }
            byte[] prefixBuf = new byte[] { 0x50, 0x50, 0x82, 0x7d, 0x13, 0x00, 0x00, 0x00 };

            Buffer.BlockCopy(BitConverter.GetBytes((ushort)buf.Length), 0, prefixBuf, 4, BitConverter.GetBytes((ushort)buf.Length).Length);

            // BitConverter.GetBytes((ushort)buf.Length).CopyTo(prefixBuf, 4);
            buf = prefixBuf.Concat(buf).ToArray();
            return buf;
        }
        private static byte[] CheckSum(byte[] p)
        {
            int l = p.Length;
            int chksum = 0;
            int i = l;
            int j = 1;

            while (i > 1)
            {
                chksum += BitConverter.ToUInt16(p, j - 1);
                if (chksum > USHRT_MAX)
                {
                    chksum -= USHRT_MAX;
                }
                i -= 2;
                j += 2;
            }

            if (i > 0)
            {
                chksum += p[l - 1];
            }

            while (chksum > USHRT_MAX)
            {
                chksum -= USHRT_MAX;
            }

            if (chksum > 0)
            {
                chksum = -chksum;
            }
            else
            {
                chksum = Math.Abs(chksum);
            }

            chksum -= 1;

            while (chksum < 0)
            {
                chksum += USHRT_MAX;
            }

            return BitConverter.GetBytes((ushort)chksum);
        }
        /// <summary>
        ///  Phương thức để xóa TCP header
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sessionId"></param>
        /// <param name="replyId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] RemoveTcpHeader(byte[] buf)
        {
            if (buf.Length < 8)
            {
                return buf;
            }

            byte[] header = new byte[] { 0x50, 0x50, 0x82, 0x7d };

            for (int i = 0; i < header.Length; i++)
            {
                if (buf[i] != header[i])
                {
                    return buf;
                }
            }

            byte[] result = new byte[buf.Length - 8];
            Array.Copy(buf, 8, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// CreateChkSum
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static int CreateChkSum(byte[] buf)
        {
            int chksum = 0;

            for (int i = 0; i < buf.Length; i += 2)
            {
                if (i == buf.Length - 1)
                {
                    chksum += buf[i];
                }
                else
                {
                    chksum += BitConverter.ToUInt16(buf, i);
                }

                chksum %= USHRT_MAX;
            }

            chksum = USHRT_MAX - chksum - 1;

            return (int)chksum;
        }

        public static bool CheckNotEventTcp(byte[] data)
        {
            try
            {
                // Remove the TCP header from the data
                data = RemoveTcpHeader(data);

                // Ensure data length is enough to read commandId and event
                if (data.Length < 6)
                {
                    return false;
                }

                // Read the commandId and event from the data
                int commandId = BitConverter.ToUInt16(data, 0);
                int eventId = BitConverter.ToUInt16(data, 4);

                // Check if the event is of type EF_ATTLOG and commandId is CMD_REG_EVENT
                return eventId == (int)COMMANDS.EF_ATTLOG && commandId == (int)COMMANDS.CMD_REG_EVENT;
            }
            catch (Exception ex)
            {
                // Log the error message and data
                Console.WriteLine($"[228] : {ex.Message}, {BitConverter.ToString(data).Replace("-", "").ToLower()}");
                return false;
            }
        }

        public static (int CommandId, int CheckSum, int SessionId, int ReplyId, int PayloadSize) DecodeTcpHeader(byte[] header)
        {
            // Ensure header length is sufficient
            if (header.Length < 8)
            {
                throw new ArgumentException("Header length is insufficient.");
            }

            // Extract the payload size (2 bytes) from the header
            int payloadSize = BitConverter.ToUInt16(header, 4);

            // Extract the relevant portion of the header for further parsing
            byte[] recvData = new byte[header.Length - 8];
            Array.Copy(header, 8, recvData, 0, recvData.Length);

            // Read values from recvData
            int commandId = BitConverter.ToUInt16(recvData, 0);
            int checkSum = BitConverter.ToUInt16(recvData, 2);
            int sessionId = BitConverter.ToUInt16(recvData, 4);
            int replyId = BitConverter.ToUInt16(recvData, 6);

            return (commandId, checkSum, sessionId, replyId, payloadSize);
        }


        private static readonly Dictionary<string, int> Commands = new Dictionary<string, int>
        {
            { "CMD_DATA", (int)COMMANDS.CMD_DATA },
            { "CMD_ACK_OK", (int)COMMANDS.CMD_ACK_OK },
            { "CMD_PREPARE_DATA", (int)COMMANDS.CMD_PREPARE_DATA },
        };

        public static string ExportErrorMessage(int commandValue)
        {
            foreach (var kvp in Commands)
            {
                if (kvp.Value == commandValue)
                {
                    return kvp.Key;
                }
            }
            return "AN UNKNOWN ERROR";
        }

        public static (string UserId, DateTime AttTime) DecodeRecordRealTimeLog52(byte[] recordData)
        {
            byte[] payload = RemoveTcpHeader(recordData);

            byte[] recvData = payload.Skip(8).ToArray();

            // Extract UserId from the first 9 bytes
            string userId = System.Text.Encoding.ASCII.GetString(recvData, 0, 9).TrimEnd('\0');

            // Extract and parse attendance time
            DateTime attTime = ParseHexToTime(recvData.Skip(26).Take(6).ToArray());

            return (userId, attTime);
        }

        private static DateTime ParseHexToTime(byte[] hexData)
        {
            // Assuming hexData is in format: YYMMDDHHMMSS
            if (hexData.Length != 6)
            {
                throw new ArgumentException("Invalid hexData length.");
            }

            string hexString = BitConverter.ToString(hexData).Replace("-", "");
            int year = int.Parse(hexString.Substring(0, 2));
            int month = int.Parse(hexString.Substring(2, 2));
            int day = int.Parse(hexString.Substring(4, 2));
            int hour = int.Parse(hexString.Substring(6, 2));
            int minute = int.Parse(hexString.Substring(8, 2));
            int second = int.Parse(hexString.Substring(10, 2));

            // Adjust year to 20xx (assuming the year is in 2000s)
            year += 2000;

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
