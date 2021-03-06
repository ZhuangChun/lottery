﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Media;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public partial class main2 : Form
    {
        enum emState
        {
            eStop,
            eRewarding,
        }
        ArrayList totalList = new ArrayList();
        ArrayList rewardedList = new ArrayList();
        // Dictionary<int, int> rewardedList = new Dictionary<int, int>();
        //ArrayList record = new ArrayList();
        int nStartId, nEndId;
        emState m_eSate = emState.eStop;

        ArrayList tmpList = new ArrayList();

        //static int tmp = 0;

        static string szRecord = "";
        static int nTimes = 0;

        SoundPlayer simpleSound;
        public Form RewardingDlg;

        const uint WM_CLOSE = 0x10;
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        System.Timers.Timer t;

        public main2()
        {
            InitializeComponent();

            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".Resources" + ".use.wav");
            simpleSound = new SoundPlayer(resourceStream);      
        }

        void CheckLuckyMan()
        {
            StreamReader objReader = new StreamReader("list.txt");
            string sLine = "";

            string szShow = "";
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                //if (sLine == null || sLine.Equals("false"))
                //{
               //     objReader.Close();
               //     return;
               // }

                if (sLine != null && !sLine.Equals("") && !sLine.Equals("-----"))
                {
                    int nId = Convert.ToInt32(sLine);
                    rewardedList.Add(nId);
                    for (int i = 0; i < totalList.Count; ++i)
                    {
                        if ((int)totalList[i] == nId)
                        {
                            totalList.RemoveAt(i);
                        }
                    }

                    szRecord += nId + "号 ";
                }
                else if (sLine == "-----")
                {
                    if (nTimes != 0)
                        szRecord += "\n";
                    nTimes++;
                    szRecord += "第" + nTimes + "次摇奖，恭喜以下同学获奖:";
                }
            }
            objReader.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.rewardCount.Text == "" || m_eSate == emState.eRewarding)
                return;

            nTimes++;
            m_eSate = emState.eRewarding;
            int nCount = Convert.ToInt32(this.rewardCount.Text);

            if (nCount > totalList.Count)
            {
                MessageBox.Show("获奖人数超出总人数，请重新输入");
                return;
            }

            tmpList.Clear();
            for (int i = 0; i < nCount; ++i)
            {
                int nKey = GetRewarder();

                tmpList.Add(nKey);
            }

            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".Resources" + ".photo05.gif");
            {
                Image image = Image.FromStream(resourceStream);
                if (image != null)
                {
                    this.pictureBox1.Image = image;
                }
            }

            simpleSound.Play();
            RewardingDlg = new GetRewarding();
            //RewardingDlg.Parent = this;
            RewardingDlg.Show();
            
            t = new System.Timers.Timer(31000);
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);
            //到达时间的时候执行事件；   
            t.AutoReset = false;
            //设置是执行一次（false）还是一直执行(true)；   
            t.Enabled = true;
            //是否执行System.Timers.Timer.Elapsed事件；

            // 写文件
            FileStream objReader = new FileStream("list.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(objReader, Encoding.UTF8);
            writer.WriteLine("-----");
            foreach (int element in tmpList)
            {
                writer.WriteLine(element);
            }

            writer.Flush();
            writer.Close();
            objReader.Close();
        }

        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        int GetRewarder()
        {
            Random rd = new Random(GetRandomSeed());
            int nIdx = rd.Next(totalList.Count - 1);

            int nId = (int)totalList[nIdx];
            rewardedList.Add(nId);
            totalList.RemoveAt(nIdx);

            return nId;
        }

        public void InitTotalList(int nStart, int nEnd)
        {
            nStartId = nStart; nEndId = nEnd;
            for (int i = nStart; i <= nEnd; ++i)
            {
                totalList.Add(i);
            }

            CheckLuckyMan();
        }

        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            // PostMessage(RewardingDlg.Handle, WM_CLOSE, 0, 0);
            //t.sto;
            CloseForm("Close");
            // RewardingDlg.Close();
            simpleSound.Stop();
            m_eSate = emState.eStop;
            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + ".Resources" + ".u.gif");
            {
                Image image = Image.FromStream(resourceStream);
                if (image != null)
                {
                    this.pictureBox1.Image = image;
                }
            }

            Form rewardListDlg = new RewardShow();
            ((RewardShow)rewardListDlg).SetShowText(tmpList);
            rewardListDlg.ShowDialog();

            if (nTimes != 1)
                szRecord += "\n";
            string szShow = "第" + nTimes + "次摇奖，恭喜以下同学获奖:";
            foreach (int id in tmpList)
            {
                szShow += id + "号 ";
            }

            szRecord += szShow;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(szRecord);
        }

        private delegate void CloseFormDelegate(string value);
        private void CloseForm(string value)
        {
            if (RewardingDlg.InvokeRequired)
            {
                CloseFormDelegate d = new CloseFormDelegate(CloseForm);
                RewardingDlg.Invoke(d, new object[] { value });
            }
            else
            {
                RewardingDlg.Close();
            }
        }  
    }

    public class MyItem : object
    {
        public string szShow;
        public override string ToString()
        {
            // TODO:  添加 MyItem.ToString 实现
            return szShow;
        }
    } 
}
