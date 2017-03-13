using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FFMPEGConverterUI
{
    class Converter : IDisposable
    {
        public string InFolderPath { get; set; }
        public string OutFolderPath { get; set; }

        private  Process m_ffmpegProcess = null;

        private  StreamReader m_streamReader = null;

        private  CultureInfo m_culture = new CultureInfo(0x0409);

        public Converter()
        {
            m_ffmpegProcess = new Process();
            m_ffmpegProcess.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\lib\\ffmpeg.exe"; // путь до ffmpeg.exe
            m_ffmpegProcess.StartInfo.UseShellExecute = false;
            m_ffmpegProcess.StartInfo.RedirectStandardError = true;
            m_ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            m_ffmpegProcess.StartInfo.CreateNoWindow = true; // скрываем процесс в панели
        }


        public  bool StartConvert(string fileName, bool repair = false)
        {
            if (repair)
            {
                m_ffmpegProcess.StartInfo.Arguments = @" -f h264 -r 12 -y -i " + InFolderPath + "\\" + fileName
                + @" -vcodec libx264 -preset ultrafast -acodec copy "+OutFolderPath+"\\" + fileName + ".mkv"; // параметры конвертации
            }
            else
            {
                m_ffmpegProcess.StartInfo.Arguments = @" -r 12 -y -i " + InFolderPath + "\\" + fileName
                + @" -vcodec copy -acodec copy " + OutFolderPath + "\\" + fileName + ".mkv"; // параметры конвертации
            }

            try
            {
                m_ffmpegProcess.Start();
                m_streamReader = m_ffmpegProcess.StandardError;

                string outString = null;
                bool ret = false;
                while (!m_streamReader.EndOfStream)
                {
                    outString = m_streamReader.ReadLine();

                    if (outString.Contains("Conversion failed!")) return false;

                    if (outString.Contains("headers:0kB"))
                    {
                        ret = true;
                    }
                }
                return ret;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                m_ffmpegProcess.WaitForExit();
                m_ffmpegProcess.Close();
            }
        }

        public void Dispose()
        {
            m_ffmpegProcess.Dispose();
        }
    }
}
