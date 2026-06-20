using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ZKLibTCP
    {
        private readonly string ip;
        private readonly int port;
        private readonly int timeout;
        private int sessionId;
        private int replyId;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private int MAX_CHUNK = 65472;


        public ZKLibTCP(string ip, int port, int timeout)
        {
            this.ip = ip;
            this.port = port;
            this.timeout = timeout;
            this.sessionId = 0;
            this.replyId = 0;
        }

        public async Task<TcpClient> CreateSocketAsync(Action<Exception> cbError = null, System.Action cbClose = null)
        {
            var tcpClient = new TcpClient
            {
                NoDelay = true
            };

            var tcs = new TaskCompletionSource<TcpClient>();

            try
            {
                // Use TcpClient's async method instead of BeginConnect for better async/await handling
                await tcpClient.ConnectAsync(this.ip, this.port);
                this.networkStream = tcpClient.GetStream();

                // Reading from the network stream should be done in a separate method or task (not started here)
                // But for now, we skip that as per your commented-out logic

                // Notify that connection was successful
                tcs.SetResult(tcpClient);
                cbClose?.Invoke();  // Callback can be removed if unnecessary here

                // Timeout logic
                using (var cts = new CancellationTokenSource(this.timeout))
                {
                    var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(this.timeout, cts.Token));
                    if (completedTask != tcs.Task)
                    {
                        tcpClient.Close();
                        throw new TimeoutException("Connection timed out.");
                    }

                    return await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                tcpClient.Close();
                tcs.SetException(ex);
                cbError?.Invoke(ex);  // Error callback
                throw;  // Re-throw the exception so it can be handled by the caller
            }
        }


        //public async Task<TcpClient> CreateSocketAsync(Action<Exception> cbError = null, System.Action cbClose = null)
        //{
        //    var tcpClient = new TcpClient();
        //    var tcs = new TaskCompletionSource<TcpClient>();

        //    tcpClient.NoDelay = true;
        //    tcpClient.BeginConnect(this.ip, this.port, async ar =>
        //    {
        //        try
        //        {
        //            tcpClient.EndConnect(ar);
        //            this.networkStream = tcpClient.GetStream();
        //            /*new Thread((obj) =>
        //            {
        //                byte[] myReadBuffer = new byte[1024*4];
        //                StringBuilder myCompleteMessage = new StringBuilder();
        //                int numberOfBytesRead = 0;

        //                // Incoming message may be larger than the buffer size. 
        //                do
        //                {


        //                    numberOfBytesRead = ((NetworkStream)obj!).Read(myReadBuffer, 0, myReadBuffer.Length);
        //                    Console.WriteLine("==============================");
        //                    Console.WriteLine(Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

        //                    var bb= myReadBuffer.Take(numberOfBytesRead).ToArray();

        //                    var rReply = ZKLibHelper.RemoveTcpHeader(bb);
        //                    if (rReply.Length == 44)
        //                    {
        //                        var bytesEnroll = rReply.Skip(8).Take(14).ToArray().Where(t => t.ToString() != "0").ToArray();
        //                        var bytesEnrollTime = rReply.Skip(34).Take(6).ToList();
        //                        var enrollTime = new DateTime(2000 + Convert.ToInt32(bytesEnrollTime[0]),
        //                            Convert.ToInt32(bytesEnrollTime[1]), Convert.ToInt32(bytesEnrollTime[2]),
        //                            Convert.ToInt32(bytesEnrollTime[3]), Convert.ToInt32(bytesEnrollTime[4]),
        //                            Convert.ToInt32(bytesEnrollTime[5]));
        //                        for (int i = 0; i < bytesEnrollTime.Count(); i++)
        //                        {
        //                            Console.WriteLine(Convert.ToInt32(bytesEnrollTime[i]));

        //                        }
        //                        var enrollID = Encoding.ASCII.GetString(bytesEnroll);
        //                        Console.WriteLine("enrollID > " + enrollID);
        //                        Console.WriteLine("enrollTime > " + enrollTime.ToString("dd/MM/yyyy HH:mm:ss"));
        //                    }
        //                    Console.WriteLine(Encoding.ASCII.GetString(rReply, 0, rReply.Length));
        //                    Console.WriteLine(string.Join(", ", rReply));


        //                    ushort command = BitConverter.ToUInt16(rReply, 0);
        //                    ushort sessionId = BitConverter.ToUInt16(rReply, 2);
        //                    int timestamp = BitConverter.ToInt32(rReply, 4);
        //                    byte[] dataPayload = new byte[rReply.Length - 8];
        //                    Array.Copy(rReply, 8, dataPayload, 0, dataPayload.Length);
        //                    string decodedPayload = Encoding.UTF8.GetString(dataPayload);
        //                    Console.WriteLine(string.Join(", ", rReply));
        //                    Console.WriteLine($"Command: {command}, SessionId: {sessionId}, Timestamp: {timestamp}, Data: {decodedPayload}");


        //                    Console.WriteLine("==============================");
        //                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

        //                }
        //                while (((NetworkStream)obj!).Socket.Connected);

        //                // Print out the received message to the console.
        //                Console.WriteLine("You received the following message : " +
        //                                  myCompleteMessage);

        //            }).Start(networkStream);*/
        //            tcs.SetResult(tcpClient);
        //            //  await ConnectAsync();
        //            cbClose?.Invoke();
        //        }
        //        catch (Exception ex)
        //        {
        //            tcs.SetException(ex);
        //            cbError?.Invoke(ex);
        //        }
        //    }, null);

        //    /*  using (var cts = new CancellationTokenSource(this.timeout))
        //      {
        //          var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
        //          if (completedTask != tcs.Task)
        //          {
        //              tcpClient.Close();
        //              throw new TimeoutException("Connection timed out.");
        //          }

        //          return await tcs.Task;
        //      }*/
        //    return tcpClient;
        //}

        public async Task<bool> ConnectAsync()
        {
            var reply = await ExecuteCmdAsync(COMMANDS.CMD_CONNECT, null);
            return reply != null;
        }

        public async Task<byte[]> ExecuteCmdAsync(COMMANDS command, byte[] data)
        {
            if (command == COMMANDS.CMD_CONNECT)
            {
                this.sessionId = 0;
                this.replyId = 0;
            }
            else
            {
                this.replyId++;
            }

            var buf = ZKLibHelper.CreateTCPHeader(command, this.sessionId, this.replyId, data);
            var reply = await WriteMessageAsync(buf, command == COMMANDS.CMD_CONNECT || command == COMMANDS.CMD_EXIT);

            var rReply = ZKLibHelper.RemoveTcpHeader(reply);
            if (command == COMMANDS.CMD_CONNECT && rReply.Length >= 6)
            {
                this.sessionId = BitConverter.ToUInt16(rReply.Skip(4).Take(2).ToArray(), 0);
            }
            return rReply;
        }

        public async Task<byte[]> WriteMessageAsync(byte[] msg, bool connect)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            if (this.networkStream == null)
                throw new InvalidOperationException("Socket is not connected.");

            this.networkStream.BeginWrite(msg, 0, msg.Length, ar =>
            {
                try
                {
                    this.networkStream.EndWrite(ar);
                    var buffer = new byte[4096];
                    this.networkStream.BeginRead(buffer, 0, buffer.Length, readAr =>
                    {
                        try
                        {
                            var bytesRead = this.networkStream.EndRead(readAr);
                            tcs.SetResult(buffer.Take(bytesRead).ToArray());
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }, null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            using (var cts = new CancellationTokenSource(this.timeout))
            {
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
                if (completedTask != tcs.Task)
                    throw new TimeoutException("Timeout while writing message.");

                return await tcs.Task;
            }

        }

        public async Task CloseSocketAsync()
        {
            try
            {
                if (this.networkStream != null)
                {
                    this.networkStream.Close();
                    this.networkStream = null;
                }

                if (this.tcpClient != null)
                {
                    this.tcpClient.Close();
                    this.tcpClient.Dispose(); // Ensure the TCP client is properly disposed
                    this.tcpClient = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing socket: {e.Message}");
            }
        }

        

        public async Task<byte[]> RequestDataAsync(byte[] msg)
        {
            var replyBuffer = new System.Collections.Generic.List<byte>();
            var tcs = new TaskCompletionSource<byte[]>();

            void OnDataReceived(IAsyncResult ar)
            {
                try
                {
                    var buffer = (byte[])ar.AsyncState;
                    replyBuffer.AddRange(buffer.Take(buffer.Length));
                    if (ZKLibHelper.CheckNotEventTcp(replyBuffer.ToArray())) return;

                    var header = ZKLibHelper.DecodeTcpHeader(replyBuffer.Take(16).ToArray());
                    if (header.CommandId == (int)COMMANDS.CMD_DATA)
                    {
                        tcs.SetResult(replyBuffer.ToArray());
                    }
                    else
                    {
                        var packetLength = BitConverter.ToUInt16(replyBuffer.Skip(4).Take(2).ToArray(), 0);
                        if (packetLength > 8)
                        {
                            tcs.SetResult(replyBuffer.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            if (this.networkStream != null)
            {
                this.networkStream.BeginRead(new byte[4096], 0, 4096, OnDataReceived, new byte[4096]);
                await this.WriteMessageAsync(msg, false);
            }

            using (var cts = new CancellationTokenSource(this.timeout))
            {
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
                if (completedTask != tcs.Task)
                    throw new TimeoutException("Timeout while receiving request data.");

                return await tcs.Task;
            }
        }

        

        public void SendChunkRequest(int start, int size)
        {
            this.replyId++;
            var reqData = new byte[8];
            BitConverter.GetBytes(start).CopyTo(reqData, 0);
            BitConverter.GetBytes(size).CopyTo(reqData, 4);
            var buf = ZKLibHelper.CreateTCPHeader(COMMANDS.CMD_DATA_RDY, this.sessionId, this.replyId, reqData);
            networkStream?.Write(buf, 0, buf.Length);
        }

        public async Task<(byte[] Data, Exception Error)> ReadWithBuffer(byte[] reqData, Action<int, int> cb = null)
        {
            // Increment reply ID
            this.replyId++;

            // Create TCP header
            byte[] buf = ZKLibHelper.CreateTCPHeader(COMMANDS.CMD_DATA_WRRQ, this.sessionId, this.replyId, reqData);
            byte[] reply = null;

            try
            {
                // Request data
                reply = await RequestData(buf);

                //Console.WriteLine("readWithBuffer > CMD_DATA_WRRQ:" + string.Join(",", reply));
            }
            catch (Exception ex)
            {
                return (null, ex);
            }

            try
            {
                // Decode TCP header
                var header = ZKLibHelper.DecodeTcpHeader(reply.Take(16).ToArray());
                //Console.WriteLine("readWithBuffer > CMD_DATA_WRRQ > header:" + string.Join(",", header));
                switch (header.CommandId)
                {
                    case (int)COMMANDS.CMD_DATA:
                        // If data command, return the data
                        return (reply.Skip(16).ToArray(), null);

                    case (int)COMMANDS.CMD_ACK_OK:
                    case (int)COMMANDS.CMD_PREPARE_DATA:
                        {
                            // Process ACK_OK or PREPARE_DATA command
                            var recvData = reply.Skip(16).ToArray();
                            int size = BitConverter.ToInt32(recvData, 1);

                            // Calculate chunk parameters
                            int remain = size % MAX_CHUNK;
                            int numberChunks = (size - remain) / MAX_CHUNK;
                            int totalPackets = numberChunks + (remain > 0 ? 1 : 0);
                            byte[] replyData = Array.Empty<byte>();

                            byte[] totalBuffer = Array.Empty<byte>();
                            byte[] realTotalBuffer = Array.Empty<byte>();

                            const int timeout = 10000;
                            CancellationTokenSource cts = new CancellationTokenSource(timeout);
                            TaskCompletionSource<(byte[] Data, Exception Error)> tcs = new TaskCompletionSource<(byte[], Exception)>();

                            void InternalCallback(byte[] data, Exception err = null)
                            {
                                cts.Cancel();
                                tcs.TrySetResult((data, err));
                            }

                            //  int packetLength = 24;
                            void HandleOnData(byte[] data)
                            {
                                try
                                {
                                    //   Console.WriteLine("readWithBuffer > CMD_DATA_WRRQ > HandleOnData" + string.Join(",", data));
                                    if (ZKLibHelper.CheckNotEventTcp(data)) return;
                                    cts.Cancel(); // Reset timeout
                                    cts = new CancellationTokenSource(timeout);

                                    totalBuffer = totalBuffer.Concat(data).ToArray();
                                    var packetLength = BitConverter.ToUInt16(totalBuffer, 4);
                                    //Console.WriteLine("HandleOnData=" + string.Join(",", data));
                                    if (totalBuffer.Length >= 8 + packetLength)
                                    {
                                        try
                                        {
                                            var segment = new ArraySegment<byte>(totalBuffer, 16, 8 + packetLength - 16).ToArray();
                                            realTotalBuffer = realTotalBuffer.Concat(segment).ToArray();
                                            totalBuffer = totalBuffer.Skip(8 + packetLength).ToArray();
                                        }
                                        catch (Exception)
                                        {

                                            throw;
                                        }

                                        // Console.WriteLine("replyData=" + System.Text.Encoding.ASCII.GetString(realTotalBuffer));
                                        if ((totalPackets > 1 && realTotalBuffer.Length == MAX_CHUNK + 8)
                                            || (totalPackets == 1 && realTotalBuffer.Length == remain + 8))
                                        {
                                            replyData = replyData.Concat(realTotalBuffer.Skip(8)).ToArray();
                                            // Console.WriteLine("replyData=" + System.Text.Encoding.ASCII.GetString(replyData));
                                            // totalBuffer = Array.Empty<byte>();
                                            realTotalBuffer = Array.Empty<byte>();

                                            totalPackets--;
                                            cb?.Invoke(replyData.Length, size);
                                            //Console.WriteLine("totalPackets=" + totalPackets);
                                            if (totalPackets <= 0)
                                            {
                                                InternalCallback(replyData);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                    throw;
                                }

                            }


                            Task handleDataTask = Task.Run(async () =>
                            {
                                {
                                    byte[] buffer = new byte[24]; // Adjust buffer size as needed

                                    int bytesRead = await this.networkStream.ReadAsync(buffer, 0, buffer.Length);

                                    if (bytesRead > 0)
                                    {
                                        HandleOnData(buffer.Take(bytesRead).ToArray());

                                    }
                                }

                                int count = 1;
                                // int bCount = 0;
                                while (totalPackets > 0)
                                {


                                    if (this.networkStream.DataAvailable)
                                    {
                                        byte[] buffer = new byte[4096]; // Adjust buffer size as needed

                                        int bytesRead = await this.networkStream.ReadAsync(buffer, 0, buffer.Length);

                                        if (bytesRead > 0)
                                        {
                                            //  bCount += bytesRead;
                                            // Console.WriteLine("readWithBuffer > CMD_DATA_WRRQ > handleDataTask > " +count);
                                            HandleOnData(buffer.Take(bytesRead).ToArray());
                                            count++;
                                        }
                                    }
                                    else
                                    {
                                        await Task.Delay(100); // Delay before checking again
                                    }
                                }
                            });

                            /* this.tcpClient.Client.BeginDisconnect(false, asyncResult =>
                             {
                                 InternalCallback(replyData, new Exception("Socket is disconnected unexpectedly"));
                             }, null);
                            */
                            for (int i = 0; i <= numberChunks; i++)
                            {
                                if (i == numberChunks)
                                {
                                    SendChunkRequest(numberChunks * MAX_CHUNK, remain);
                                }
                                else
                                {
                                    SendChunkRequest(i * MAX_CHUNK, MAX_CHUNK);
                                }
                            }

                            try
                            {
                                return await tcs.Task;
                            }
                            catch (OperationCanceledException)
                            {
                                return (null, new Exception("TIMEOUT WHEN RECEIVING PACKET"));
                            }
                            finally
                            {
                                // this.tcpClient.Client.EndDisconnect(null);
                            }
                        }

                    default:
                        return (null, new Exception("ERROR_IN_UNHANDLE_CMD " + ZKLibHelper.ExportErrorMessage(header.CommandId)));
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task FreeDataAsync()
        {
            await ExecuteCmdAsync(COMMANDS.CMD_FREE_DATA, null);
        }

      

        public async Task EnableDeviceAsync()
        {
            await ExecuteCmdAsync(COMMANDS.CMD_ENABLEDEVICE, null);
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await ExecuteCmdAsync(COMMANDS.CMD_EXIT, null);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Disconnect error: {e.Message}");
                // Swallow the exception but ensure socket is still closed.
            }
            finally
            {
                await CloseSocketAsync(); // Only call this once.
            }
        }

        public async Task<(int userCounts, int logCounts, int logCapacity)> GetInfoAsync()
        {
            var data = await ExecuteCmdAsync(COMMANDS.CMD_GET_FREE_SIZES, null);

            return (
                userCounts: BitConverter.ToInt32(data.Skip(24).Take(4).ToArray(), 0),
                logCounts: BitConverter.ToInt32(data.Skip(40).Take(4).ToArray(), 0),
                logCapacity: BitConverter.ToInt32(data.Skip(72).Take(4).ToArray(), 0)
            );
        }

        


        public async Task ClearAttendanceLogAsync()
        {
            await ExecuteCmdAsync(COMMANDS.CMD_CLEAR_ATTLOG, null);
        }

        public async Task GetRealTimeLogsAsync(Action<object> cb = null)
        {
            this.replyId++;
            var buf = ZKLibHelper.CreateTCPHeader(COMMANDS.CMD_REG_EVENT, this.sessionId, this.replyId, new byte[] { 0x01, 0x00, 0x00, 0x00 });

            this.networkStream?.Write(buf, 0, buf.Length);

            this.networkStream?.BeginRead(new byte[4096], 0, 4096, ar =>
            {
                var data = (byte[])ar.AsyncState;
                if (!ZKLibHelper.CheckNotEventTcp(data)) return;

                if (data.Length > 16)
                {
                    cb?.Invoke(ZKLibHelper.DecodeRecordRealTimeLog52(data));
                }
            }, new byte[4096]);
        }

        public async Task<byte[]> RequestData(byte[] msg)
        {
            // Create a task completion source to handle the async operation
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            byte[] replyBuffer = Array.Empty<byte>();
            CancellationTokenSource cts = new CancellationTokenSource(this.timeout);
            Timer timer = null;

            // Define the internal callback to complete the task
            void InternalCallback(byte[] data)
            {
                // timer?.Change(Timeout.Infinite, Timeout.Infinite);
                // timer?.Dispose();
                tcs.SetResult(data);
            }

            // Define the handler for incoming data
            async Task HandleOnData(NetworkStream stream)
            {
                byte[] buffer = new byte[4096]; // Adjust buffer size as needed
                int bytesRead;








                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] data = buffer.Take(bytesRead).ToArray();
                    replyBuffer = replyBuffer.Concat(data).ToArray();

                    if (ZKLibHelper.CheckNotEventTcp(replyBuffer)) return;

                    var header = ZKLibHelper.DecodeTcpHeader(replyBuffer.Take(16).ToArray());

                    if (header.CommandId == (int)COMMANDS.CMD_DATA)
                    {
                        // Set a short timeout for the data command
                        timer = new Timer(state => InternalCallback(replyBuffer), null, 1000, Timeout.Infinite);
                    }
                    else
                    {

                        int packetLength = BitConverter.ToUInt16(data, 4);
                        if (packetLength > 8)
                        {
                            InternalCallback(replyBuffer);
                        }
                        else
                        {
                            // Set a longer timeout for other commands
                            timer = new Timer(state => tcs.SetException(new Exception("TIMEOUT_ON_RECEIVING_REQUEST_DATA")), null, this.timeout, Timeout.Infinite);

                        }
                    }
                }
            }

            try
            {
                // using (NetworkStream stream = this.tcpClient.GetStream())
                {
                    // Send the message
                    // await networkStream.WriteAsync(msg, 0, msg.Length);

                    // Set a timeout for receiving a response
                    // timer = new Timer(state => tcs.SetException(new Exception("TIMEOUT_IN_RECEIVING_RESPONSE_AFTER_REQUESTING_DATA")), null, this.timeout, Timeout.Infinite);

                    // Read data
                    // await HandleOnData(networkStream);



                    this.networkStream.BeginWrite(msg, 0, msg.Length, ar =>
                    {
                        try
                        {
                            this.networkStream.EndWrite(ar);
                            var buffer = new byte[4096];
                            this.networkStream.BeginRead(buffer, 0, buffer.Length, readAr =>
                            {
                                try
                                {
                                    var bytesRead = this.networkStream.EndRead(readAr);
                                    tcs.SetResult(buffer.Take(bytesRead).ToArray());
                                }
                                catch (Exception ex)
                                {
                                    tcs.SetException(ex);
                                }
                            }, null);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    }, null);

                    /* using (var cts = new CancellationTokenSource(this.timeout))
                     {
                         var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
                         if (completedTask != tcs.Task)
                             throw new TimeoutException("Timeout while writing message.");

                         return await tcs.Task;
                     }*/

                    // Wait for the task to complete or timeout
                    return await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                // Clean up in case of an exception
                timer?.Dispose();
                throw ex;
            }
        }

        byte[] GetPage(byte[] buffer, int pageNumber, int pageSize)
        {
            int start = pageNumber * pageSize;
            int length = Math.Min(pageSize, buffer.Length - start);

            if (start >= buffer.Length)
            {
                return new byte[0]; // Return an empty array if the start is beyond the buffer length
            }

            byte[] page = new byte[length];
            Array.Copy(buffer, start, page, 0, length);
            return page;
        }

       

        // Helper method to parse time, assumed to be implemented similarly to parseTimeToDate in JS
        public static DateTime ParseTimeToDate(int time)
        {
            int second = time % 60;
            time = (time - second) / 60;
            int minute = time % 60;
            time = (time - minute) / 60;
            int hour = time % 24;
            time = (time - hour) / 24;
            int day = time % 31 + 1;
            time = (time - (day - 1)) / 31;
            int month = time % 12;
            time = (time - month) / 12;
            int year = time + 2000;

            return new DateTime(year, month + 1, day, hour, minute, second);
        }

    }
}
