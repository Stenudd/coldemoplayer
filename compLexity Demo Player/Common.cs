﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics; // Process
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Windows.Controls;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Windows;
using System.Windows.Data;

namespace compLexity_Demo_Player
{
    public static class Common
    {
        private static Byte[] MungeTable3 =
        {
            0x20, 0x07, 0x13, 0x61,
            0x03, 0x45, 0x17, 0x72,
            0x0A, 0x2D, 0x48, 0x0C,
            0x4A, 0x12, 0xA9, 0xB5
        };

        private static UInt32 FlipBytes32(UInt32 value)
        {
            return (((value & 0xFF000000) >> 24) | ((value & 0x00FF0000) >> 8) | ((value & 0x0000FF00) << 8) | ((value & 0x000000FF) << 24));
        }

        public static UInt32 UnMunge3(UInt32 value, Int32 z) 
        {
            z = (0xFF - z) & 0xFF;
            value = (UInt32)(value ^ z);

            Byte[] temp = BitConverter.GetBytes(value);

            for (Int32 i = 0; i < 4; i++)
            {
                temp[i] ^= (Byte)((((Int32)MungeTable3[i & 0x0F] | i << i) | i) | 0xA5);
            }

            return (UInt32)(FlipBytes32(BitConverter.ToUInt32(temp, 0)) ^ (~z));
        }

        public static void WebDownloadBinaryFile(String address, String outputFileName, Procedure<Int32> downloadProgress)
        {
            const Int32 bufferSize = 1024;

            WebRequest request = WebRequest.Create(address);
            using (WebResponse response = request.GetResponse())
            {
                using (Stream input = response.GetResponseStream())
                {
                    using (FileStream output = File.Create(outputFileName))
                    {
                        using (BinaryReader reader = new BinaryReader(input))
                        {
                            using (BinaryWriter writer = new BinaryWriter(output))
                            {
                                Int64 fileLength = response.ContentLength;
                                Int32 bytesRead = 0;

                                while (true)
                                {
                                    Byte[] buffer = reader.ReadBytes(bufferSize);

                                    bytesRead += buffer.Length;

                                    if (downloadProgress != null)
                                    {
                                        downloadProgress((Int32)(bytesRead / (Single)fileLength * 100.0f));
                                    }

                                    if (buffer.Length > 0)
                                    {
                                        writer.Write(buffer);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static String WebDownloadTextFile(String url)
        {
            String s = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    s = client.DownloadString(url);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception)
            {
                // we want to return null if an error occurs
            }

            return s;
        }

        public static void XmlFileSerialize(String fileName, Object o, Type type)
        {
            XmlFileSerialize(fileName, o, type, null);
        }

        public static void XmlFileSerialize(String fileName, Object o, Type type, Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
            {
                serializer = new XmlSerializer(type);
            }
            else
            {
                serializer = new XmlSerializer(type, extraTypes);
            }

            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, o);
            }
        }

        public static Object XmlFileDeserialize(String fileName, Type type)
        {
            return XmlFileDeserialize(fileName, type, null);
        }

        public static Object XmlFileDeserialize(String fileName, Type type, Type[] extraTypes)
        {
            Object result = null;

            using (TextReader stream = new StreamReader(fileName))
            {
                result = XmlFileDeserialize((StreamReader)stream, type, extraTypes);
            }

            return result;
        }

        public static Object XmlFileDeserialize(StreamReader stream, Type type, Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
            {
                serializer = new XmlSerializer(type);
            }
            else
            {
                serializer = new XmlSerializer(type, extraTypes);
            }

            return serializer.Deserialize(stream);
        }

        public static Process FindProcess(String fileNameWithoutExtension)
        {
            return FindProcess(fileNameWithoutExtension, null);
        }

        public static Process FindProcess(String fileNameWithoutExtension, String exeFullPath)
        {
            return FindProcess(fileNameWithoutExtension, exeFullPath, -1);
        }

        public static Process FindProcess(String fileNameWithoutExtension, String exeFullPath, Int32 ignoreId)
        {
            Process[] processes = Process.GetProcessesByName(fileNameWithoutExtension);

            if (exeFullPath == null)
            {
                if (processes.Length > 0)
                {
                    return processes[0];
                }

                return null;
            }

            foreach (Process p in processes)
            {
                String compare = exeFullPath; // just incase the following fucks up...

                // ugly fix for weird error message.
                // possible cause: accessing MainModule too soon after process has executed.
                // FileName seems to be read just find though so...
                try
                {
                    compare = p.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                }

                if (p.Id != ignoreId && String.Equals(exeFullPath, compare, StringComparison.CurrentCultureIgnoreCase))
                {
                    return p;
                }
            }

            return null;
        }

        public static String SanitisePath(String s)
        {
            return s.Replace('/', '\\');
        }

        public static MessageWindow.Result Message(String message)
        {
            return Message(null, message);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message)
        {
            return Message(owner, message, null);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex)
        {
            return Message(owner, message, ex, 0);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex, MessageWindow.Flags flags)
        {
            return Message(owner, message, ex, flags, null);
        }

        public static MessageWindow.Result Message(System.Windows.Window owner, String message, Exception ex, MessageWindow.Flags flags, Procedure<MessageWindow.Result> setResult)
        {
            MessageWindow window = new MessageWindow(message, ex, flags, setResult);
            window.ShowInTaskbar = (owner == null);
            window.Owner = owner;
            window.WindowStartupLocation = (owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner);
            window.ShowDialog();

            return window.CloseResult;
        }

        /// <summary>
        /// Returns a string representation of the specified number in the format H:MM:SS.
        /// </summary>
        /// <param name="d">The duration in seconds.</param>
        /// <returns></returns>
        public static String DurationString(Single d)
        {
            TimeSpan ts = new TimeSpan(0, 0, (Int32)d);
            return String.Format("{0}{1}:{2}", (ts.Hours > 0 ? (ts.Hours.ToString() + ":") : ""), ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
        }

        public static String ReadNullTerminatedString(BinaryReader br, Int32 charCount)
        {
            Int64 startingByteIndex = br.BaseStream.Position;
            String s = ReadNullTerminatedString(br);

            Int32 bytesToSkip = charCount - (Int32)(br.BaseStream.Position - startingByteIndex);
            br.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);

            return s;
        }

        // TODO: replace with bitbuffer algorithm? benchmark and see which is faster
        public static String ReadNullTerminatedString(BinaryReader br)
        {
            List<Char> chars = new List<Char>();

            while (true)
            {
                Char c = br.ReadChar();

                if (c == '\0')
                    break;

                chars.Add(c);
            }

            return new String(chars.ToArray());
        }

        public static Boolean ZipFileExists(String zipFileName, String fileFullPath)
        {
            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    String path = Path.GetDirectoryName(fileFullPath);
                    String fileName = Path.GetFileName(fileFullPath);

                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        if (Path.GetDirectoryName(entry.Name) == path && Path.GetFileName(entry.Name) == fileName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static Boolean ZipFolderExists(String zipFileName, String folderPath)
        {
            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        if (Path.GetDirectoryName(entry.Name) == folderPath)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extracts all files within a ZIP file folder that match the extension wildcard. If a destination file already exists then the file isn't extracted.
        /// </summary>
        /// <param name="zipFileName"></param>
        /// <param name="sourcePath"></param>
        /// <param name="fileExtension"></param>
        /// <param name="destinationPath"></param>
        /// <example>ZipExtractFileType("movies.zip", "myfolder", "avi", "C:\\Movies");</example>
        public static void ZipExtractFileType(String zipFileName, String sourcePath, String fileExtension, String destinationPath)
        {
            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        if (Path.GetDirectoryName(entry.Name) == sourcePath && Path.GetExtension(entry.Name) == ("." + fileExtension))
                        {
                            String fileName = destinationPath + "\\" + Path.GetFileName(entry.Name);

                            if (File.Exists(fileName))
                            {
                                // don't overwrite
                                continue;
                            }

                            ZipStreamWriteFile(stream, fileName);
                        }
                    }
                }
            }
        }

        public static void ZipExtractFile(String zipFileName, String fileFullPath, String destinationPath)
        {
            String path = Path.GetDirectoryName(fileFullPath);
            String fileName = Path.GetFileName(fileFullPath);

            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        if (Path.GetDirectoryName(entry.Name) == path && Path.GetFileName(entry.Name) == fileName)
                        {
                            ZipStreamWriteFile(stream, destinationPath + "\\" + fileName);
                            stream.Close();
                            return;
                        }
                    }
                }
            }
        }

        public static void ZipExtractFolder(String zipFileName, String folderPath, String destinationPath)
        {
            using (FileStream fileStream = File.OpenRead(zipFileName))
            {
                using (ZipInputStream stream = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = stream.GetNextEntry()) != null)
                    {
                        String directoryName = Path.GetDirectoryName(entry.Name);
                        String fileName = Path.GetFileName(entry.Name);

                        if (directoryName.StartsWith(folderPath))
                        {
                            String newDirectoryName = directoryName.Replace(folderPath, "");

                            if (directoryName != folderPath)
                            {
                                if (!Directory.Exists(destinationPath + newDirectoryName))
                                {
                                    Directory.CreateDirectory(destinationPath + newDirectoryName);
                                }
                            }

                            if (fileName != String.Empty)
                            {
                                ZipStreamWriteFile(stream, destinationPath + newDirectoryName + "\\" + fileName);
                            }
                        }
                    }
                }
            }
        }

        private static void ZipStreamWriteFile(ZipInputStream stream, String fileFullPath)
        {
            const Int32 blockSize = 4096;

            using (FileStream streamWriter = File.Create(fileFullPath))
            {
                Byte[] data = new Byte[blockSize];

                while (true)
                {
                    Int32 bytesRead = stream.Read(data, 0, data.Length);

                    if (bytesRead > 0)
                    {
                        streamWriter.Write(data, 0, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }

        public static Int32 LogBase2(Int32 number)
        {
            Debug.Assert(number == 1 || number % 2 == 0);

            Int32 result = 0;

            while ((number >>= 1) != 0)
            {
                result++;
            }

            return result;
        }

        public static void AbortThread(System.Threading.Thread thread)
        {
            Debug.Assert(thread != null);

            if (!thread.IsAlive)
            {
                return;
            }

            thread.Abort();
            thread.Join();

           /* do
            {
                thread.Abort();
            }
            while (!thread.Join(100));*/
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, Function<TSource, bool> predicate)
        {
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return default(TSource);
        }
    }

    [ValueConversion(typeof(Object), typeof(Object))]
    public class NullConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            return value;
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Single), typeof(String))]
    public class TimestampConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return Common.DurationString((Single)value);
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Func
    public delegate TResult Function<T, TResult>(T arg);
    public delegate TResult Function<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Function<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Function<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    // Action
    public delegate void Procedure();
    public delegate void Procedure<T1>(T1 arg);
    public delegate void Procedure<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Procedure<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Procedure<T1, T2, T3, T4>(T1 arg1, T2 arg2, T4 arg4);
}