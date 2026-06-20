using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Connects.BL
{
    public class Folder_File_Tool
    {
        public static string folder = @"System\WriteAll\";
        public static string fileDevice = "Device.txt";
        public static string fileHost = "Host.txt";
        public static string fileDateDevice => "device_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";


        public static void Write(string file, string data)
        {
            using (var sw = new StreamWriter(file, true, Encoding.UTF8))
            {
                sw.Write(data);
            }
        }

        public static void Create_Write_File(string protocolName, bool isDevice, string data)
        {
            string folder = string.Concat(Folder_File_Tool.folder, protocolName, @"\");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string file = string.Empty;
            if (isDevice)
                file = folder + fileDevice;
            else
                file = folder + fileHost;


            if (!File.Exists(file))
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(data);
                sw.Flush();
                fs.Close();
            }
            else
            {
                string currentData = File.ReadAllText(file);
                string newData = currentData + data;
                File.WriteAllText(file, newData);
            }
        }


        public static void CreateWriteFileByDate(string protocolName, bool isDevice, string data)
        {
            string folder = string.Concat(Folder_File_Tool.folder, protocolName, @"\");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string prefix = isDevice ? "device" : "host";

            string file = Path.Combine(folder, $"{prefix}_{DateTime.Now:yyyyMMdd}.txt");
            Write(file, data + $"\r\n");
        }

        public static DateTime InsertDate(string value)
        {
            try
            {
                var date = value.Substring(0, 4) + "-" + value.Substring(4, 2) + "-" + value.Substring(6, 2);
                var time = value.Substring(8, 2) + ":" + value.Substring(10, 2) + ":" + value.Substring(12, 2);
                return DateTime.Parse(date + " " + time);
            }
            catch
            {
                return DateTimeServer.Get_DateServerByEntity();
            }
        }

        public static void ParseResult(string value, out double? result, out string posneg)
        {
            posneg = string.Empty;
            result = null;
            try
            {
                result = ParseToDouble(value);

                // Giới hạn 1 chữ số thập phân
                if (result.HasValue)
                    result = Math.Round(result.Value, 2, MidpointRounding.AwayFromZero);
            }
            catch
            {
                posneg = value;
            }
        }

        public static double ParseToDouble(string value)
        {
            return double.Parse(value, GetCultureInfo());
        }

        public static CultureInfo GetCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            return cultureInfo;
        }

    }

    public static class RichTextStripper
    {
        public class StackEntry
        {
            public int NumberOfCharactersToSkip { get; set; }
            public bool Ignorable { get; set; }

            public StackEntry(int numberOfCharactersToSkip, bool ignorable)
            {
                NumberOfCharactersToSkip = numberOfCharactersToSkip;
                Ignorable = ignorable;
            }
        }

        public static readonly Regex _rtfRegex = new Regex(@"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static readonly List<string> destinations = new List<string>
    {
        "aftncn","aftnsep","aftnsepc","annotation","atnauthor","atndate","atnicn","atnid",
        "atnparent","atnref","atntime","atrfend","atrfstart","author","background",
        "bkmkend","bkmkstart","blipuid","buptim","category","colorschememapping",
        "colortbl","comment","company","creatim","datafield","datastore","defchp","defpap",
        "do","doccomm","docvar","dptxbxtext","ebcend","ebcstart","factoidname","falt",
        "fchars","ffdeftext","ffentrymcr","ffexitmcr","ffformat","ffhelptext","ffl",
        "ffname","ffstattext","field","file","filetbl","fldinst","fldrslt","fldtype",
        "fname","fontemb","fontfile","fonttbl","footer","footerf","footerl","footerr",
        "footnote","formfield","ftncn","ftnsep","ftnsepc","g","generator","gridtbl",
        "header","headerf","headerl","headerr","hl","hlfr","hlinkbase","hlloc","hlsrc",
        "hsv","htmltag","info","keycode","keywords","latentstyles","lchars","levelnumbers",
        "leveltext","lfolevel","linkval","list","listlevel","listname","listoverride",
        "listoverridetable","listpicture","liststylename","listtable","listtext",
        "lsdlockedexcept","macc","maccPr","mailmerge","maln","malnScr","manager","margPr",
        "mbar","mbarPr","mbaseJc","mbegChr","mborderBox","mborderBoxPr","mbox","mboxPr",
        "mchr","mcount","mctrlPr","md","mdeg","mdegHide","mden","mdiff","mdPr","me",
        "mendChr","meqArr","meqArrPr","mf","mfName","mfPr","mfunc","mfuncPr","mgroupChr",
        "mgroupChrPr","mgrow","mhideBot","mhideLeft","mhideRight","mhideTop","mhtmltag",
        "mlim","mlimloc","mlimlow","mlimlowPr","mlimupp","mlimuppPr","mm","mmaddfieldname",
        "mmath","mmathPict","mmathPr","mmaxdist","mmc","mmcJc","mmconnectstr",
        "mmconnectstrdata","mmcPr","mmcs","mmdatasource","mmheadersource","mmmailsubject",
        "mmodso","mmodsofilter","mmodsofldmpdata","mmodsomappedname","mmodsoname",
        "mmodsorecipdata","mmodsosort","mmodsosrc","mmodsotable","mmodsoudl",
        "mmodsoudldata","mmodsouniquetag","mmPr","mmquery","mmr","mnary","mnaryPr",
        "mnoBreak","mnum","mobjDist","moMath","moMathPara","moMathParaPr","mopEmu",
        "mphant","mphantPr","mplcHide","mpos","mr","mrad","mradPr","mrPr","msepChr",
        "mshow","mshp","msPre","msPrePr","msSub","msSubPr","msSubSup","msSubSupPr","msSup",
        "msSupPr","mstrikeBLTR","mstrikeH","mstrikeTLBR","mstrikeV","msub","msubHide",
        "msup","msupHide","mtransp","mtype","mvertJc","mvfmf","mvfml","mvtof","mvtol",
        "mzeroAsc","mzeroDesc","mzeroWid","nesttableprops","nextfile","nonesttables",
        "objalias","objclass","objdata","object","objname","objsect","objtime","oldcprops",
        "oldpprops","oldsprops","oldtprops","oleclsid","operator","panose","password",
        "passwordhash","pgp","pgptbl","picprop","pict","pn","pnseclvl","pntext","pntxta",
        "pntxtb","printim","private","propname","protend","protstart","protusertbl","pxe",
        "result","revtbl","revtim","rsidtbl","rxe","shp","shpgrp","shpinst",
        "shppict","shprslt","shptxt","sn","sp","staticval","stylesheet","subject","sv",
        "svb","tc","template","themedata","title","txe","ud","upr","userprops",
        "wgrffmtfilter","windowcaption","writereservation","writereservhash","xe","xform",
        "xmlattrname","xmlattrvalue","xmlclose","xmlname","xmlnstbl",
        "xmlopen"
    };

        public static readonly Dictionary<string, string> specialCharacters = new Dictionary<string, string>
    {
        { "par", "\n" },
        { "sect", "\n\n" },
        { "page", "\n\n" },
        { "line", "\n" },
        { "tab", "\t" },
        { "emdash", "\u2014" },
        { "endash", "\u2013" },
        { "emspace", "\u2003" },
        { "enspace", "\u2002" },
        { "qmspace", "\u2005" },
        { "bullet", "\u2022" },
        { "lquote", "\u2018" },
        { "rquote", "\u2019" },
        { "ldblquote", "\u201C" },
        { "rdblquote", "\u201D" },
    };

        public static string StripRichTextFormat(string inputRtf)
        {
            if (inputRtf == null)
            {
                return null;
            }

            string returnString;

            var stack = new Stack<StackEntry>();
            bool ignorable = false;              // Whether this group (and all inside it) are "ignorable".
            int ucskip = 1;                      // Number of ASCII characters to skip after a unicode character.
            int curskip = 0;                     // Number of ASCII characters left to skip
            var outList = new List<string>();    // Output buffer.

            MatchCollection matches = _rtfRegex.Matches(inputRtf);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string word = match.Groups[1].Value;
                    string arg = match.Groups[2].Value;
                    string hex = match.Groups[3].Value;
                    string character = match.Groups[4].Value;
                    string brace = match.Groups[5].Value;
                    string tchar = match.Groups[6].Value;

                    if (!String.IsNullOrEmpty(brace))
                    {
                        curskip = 0;
                        if (brace == "{")
                        {
                            // Push state
                            stack.Push(new StackEntry(ucskip, ignorable));
                        }
                        else if (brace == "}")
                        {
                            // Pop state
                            StackEntry entry = stack.Pop();
                            ucskip = entry.NumberOfCharactersToSkip;
                            ignorable = entry.Ignorable;
                        }
                    }
                    else if (!String.IsNullOrEmpty(character)) // \x (not a letter)
                    {
                        curskip = 0;
                        if (character == "~")
                        {
                            if (!ignorable)
                            {
                                outList.Add("\xA0");
                            }
                        }
                        else if ("{}\\".Contains(character))
                        {
                            if (!ignorable)
                            {
                                outList.Add(character);
                            }
                        }
                        else if (character == "*")
                        {
                            ignorable = true;
                        }
                    }
                    else if (!String.IsNullOrEmpty(word)) // \foo
                    {
                        curskip = 0;
                        if (destinations.Contains(word))
                        {
                            ignorable = true;
                        }
                        else if (ignorable)
                        {
                        }
                        else if (specialCharacters.ContainsKey(word))
                        {
                            outList.Add(specialCharacters[word]);
                        }
                        else if (word == "uc")
                        {
                            ucskip = Int32.Parse(arg);
                        }
                        else if (word == "u")
                        {
                            int c = Int32.Parse(arg);
                            if (c < 0)
                            {
                                c += 0x10000;
                            }
                            //Ein gültiger UTF32-Wert ist zwischen 0x000000 und 0x10ffff (einschließlich) und sollte keine Ersatzcodepunktwerte (0x00d800 ~ 0x00dfff)
                            if (c >= 0x000000 && c <= 0x10ffff && (c < 0x00d800 || c > 0x00dfff))
                                outList.Add(Char.ConvertFromUtf32(c));
                            else outList.Add("?");
                            curskip = ucskip;
                        }
                    }
                    else if (!String.IsNullOrEmpty(hex)) // \'xx
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            int c = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                            outList.Add(Char.ConvertFromUtf32(c));
                        }
                    }
                    else if (!String.IsNullOrEmpty(tchar))
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            outList.Add(tchar);
                        }
                    }
                }
            }
            else
            {
                // Didn't match the regex
                returnString = inputRtf;
            }

            returnString = String.Join(String.Empty, outList.ToArray());

            return returnString;
        }
    }
}
