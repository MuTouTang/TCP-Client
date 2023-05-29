using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlantWall.TCP.Publisher
{
    class Program
    {
        //创建1个客户端套接字和1个负责监听服务端请求的线程  
        static Thread ThreadClient = null;
        static Socket SocketClient = null;
        static void Main(string[] args)
        {
            try
            {
                int port = 1885;
                string host = "192.168.43.205";//服务器端ip地址
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                //定义一个套接字监听  
                SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //数据库查询标记设置
                string connectString = "Data Source=127.0.0.1;Initial Catalog=PlantWallDB;Persist Security Info=True;User ID=sa;Password=123th123";

                int Start = 0, CarbonDioxide = 0, Humidity = 0, Temperature = 0, LightIntensity = 0, PH = 0, Mosquitoligth = 0, Camera = 0, Oxygen = 0, TVOC = 0, SoilMoisture = 0, SoilTemper = 0, CO = 0, Humidifier = 0;
                bool sign = false, signC = false, signH = false, signT = false, signL = false, signP = false, signM = false, signC1 = false, signO = false, signT1 = false, signS = false, signS1 = false, signC2 = false,signH1=false;
                int Water = 0, Nitrogenous = 0, Phosphorus = 0, Potassium = 0, DWater = 0, DNitrogenous = 0, DPhosphorus = 0, DPotassium = 0;
                int hour, minute, second;

                using (SqlConnection sqlConnection = new SqlConnection(connectString))
                {
                    string sql = "select Water,Nitrogenous,Phosphorus,Potassium from [PlantWallDB].[dbo].[AddElement] where account='admin'";
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Water = reader.GetInt32(0);
                                Nitrogenous = reader.GetInt32(1);
                                Phosphorus = reader.GetInt32(2);
                                Potassium = reader.GetInt32(3);
                            }
                        }
                    }
                }

                try
                {
                    //客户端套接字连接到网络节点上，用的是Connect  
                    SocketClient.Connect(ipEndPoint);
                }
                catch (Exception)
                {
                    Console.WriteLine("连接失败！");
                    Console.ReadLine();
                    return;
                }
                ThreadClient = new Thread(RecvSMsg);
                ThreadClient.IsBackground = true;
                ThreadClient.Start();
                //Thread.Sleep(1000);
                //Console.WriteLine("请输入内容<按Enter键发送>：");
                while (true)
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connectString))
                    {
                        string sql = "select Start,CarbonDioxide,Humidity,Temperature,LightIntensity,PH,Mosquitoligth,Camera,Oxygen,TVOC,SoilMoisture,SoilTemper,CO,Humidifier from [PlantWallDB].[dbo].[Instruction] where account='admin'";
                        sqlConnection.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Start = reader.GetInt32(0);
                                    CarbonDioxide = reader.GetInt32(1);
                                    Humidity = reader.GetInt32(2);
                                    Temperature = reader.GetInt32(3);
                                    LightIntensity = reader.GetInt32(4);
                                    PH = reader.GetInt32(5);
                                    Mosquitoligth = reader.GetInt32(6);
                                    Camera = reader.GetInt32(7);
                                    Oxygen = reader.GetInt32(8);
                                    TVOC = reader.GetInt32(9);
                                    SoilMoisture = reader.GetInt32(10);
                                    SoilTemper = reader.GetInt32(11);
                                    CO = reader.GetInt32(12);
                                    Humidifier = reader.GetInt32(13);
                                }
                            }
                        }
                        sql = "select Water,Nitrogenous,Phosphorus,Potassium from [PlantWallDB].[dbo].[AddElement] where account='admin'";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    DWater = reader.GetInt32(0);
                                    DNitrogenous = reader.GetInt32(1);
                                    DPhosphorus = reader.GetInt32(2);
                                    DPotassium = reader.GetInt32(3);
                                }
                            }
                        }

                        if (Start == 1 && sign == true)
                        {
                            ClientSendMsg("pub READ*");
                            sign = false;
                        }
                        else if (Start == 0 && sign == false)
                        {
                            ClientSendMsg("pub STOP*");
                            sign = true;
                        }

                        if (CarbonDioxide == 1 && signC == true)
                        {
                            ClientSendMsg("pub CarbonDioxide_1*");
                            signC = false;
                        }
                        else if (CarbonDioxide == 0 && signC == false)
                        {
                            ClientSendMsg("pub CarbonDioxide_0*");
                            signC = true;
                        }

                        if (Humidity == 1 && signH == true)
                        {
                            ClientSendMsg("pub Humidity_1*");
                            signH = false;
                        }
                        else if (Humidity == 0 && signH == false)
                        {
                            ClientSendMsg("pub Humidity_0*");
                            signH = true;
                        }

                        if (Temperature == 1 && signT == true)
                        {
                            ClientSendMsg("pub Temperature_1*");
                            signT = false;
                        }
                        else if (Temperature == 0 && signT == false)
                        {
                            ClientSendMsg("pub Temperature_0*");
                            signT = true;
                        }

                        if (LightIntensity == 1 && signL == true)
                        {
                            ClientSendMsg("pub LED1_1*");
                            signL = false;
                        }
                        else if (LightIntensity == 0 && signL == false)
                        {
                            ClientSendMsg("pub LED1_0*");
                            signL = true;
                        }

                        if (PH == 1 && signP == true)
                        {
                            ClientSendMsg("pub PH_1*");
                            signP = false;
                        }
                        else if (PH == 0 && signP == false)
                        {
                            ClientSendMsg("pub PH_0*");
                            signP = true;
                        }

                        if (Mosquitoligth == 1 && signM == true)
                        {
                            ClientSendMsg("pub LED2_1*");
                            signM = false;
                        }
                        else if (Mosquitoligth == 0 && signM == false)
                        {
                            ClientSendMsg("pub LED2_0*");
                            signM = true;
                        }

                        if (Camera == 1 && signC1 == true)
                        {
                            ClientSendMsg("pub Camera_1*");
                            signC1 = false;
                        }
                        else if (Camera == 0 && signC1 == false)
                        {
                            ClientSendMsg("pub Camera_0*");
                            signC1 = true;
                        }

                        if (Oxygen == 1 && signO == true)
                        {
                            ClientSendMsg("pub Oxygen_1*");
                            signO = false;
                        }
                        else if (Oxygen == 0 && signO == false)
                        {
                            ClientSendMsg("pub Oxygen_0*");
                            signO = true;
                        }

                        if (TVOC == 1 && signT1 == true)
                        {
                            ClientSendMsg("pub TVOC_1*");
                            signT1 = false;
                        }
                        else if (TVOC == 0 && signT1 == false)
                        {
                            ClientSendMsg("pub TVOC_0*");
                            signT1 = true;
                        }

                        if (SoilMoisture == 1 && signS == true)
                        {
                            ClientSendMsg("pub SoilMoisture_1*");
                            signS = false;
                        }
                        else if (SoilMoisture == 0 && signS == false)
                        {
                            ClientSendMsg("pub SoilMoisture_0*");
                            signS = true;
                        }

                        if (SoilTemper == 1 && signS1 == true)
                        {
                            ClientSendMsg("pub SoilTemper_1*");
                            signS1 = false;
                        }
                        else if (SoilTemper == 0 && signS1 == false)
                        {
                            ClientSendMsg("pub SoilTemper_0*");
                            signS1 = true;
                        }

                        if (CO == 1 && signC2 == true)
                        {
                            ClientSendMsg("pub CO_1*");
                            signC2 = false;
                        }
                        else if (CO == 0 && signC2 == false)
                        {
                            ClientSendMsg("pub CO_0*");
                            signC2 = true;
                        }

                        if (Humidifier == 1 && signH1 == true)
                        {
                            ClientSendMsg("pub Humidifier_1*");
                            signH1 = false;
                        }
                        else if (Humidifier == 0 && signH1 == false)
                        {
                            ClientSendMsg("pub Humidifier_0*");
                            signH1 = true;
                        }

                        if (Water != DWater)
                        {
                            ClientSendMsg($"pub Water {DWater}*");
                            Water = DWater;
                        }

                        if (Nitrogenous != DNitrogenous)
                        {
                            ClientSendMsg($"pub fertilize {DNitrogenous}*");
                            Nitrogenous = DNitrogenous;
                        }

                        if (Phosphorus != DPhosphorus)
                        {
                            ClientSendMsg($"pub fertilize {DPhosphorus}*");
                            Phosphorus = DPhosphorus;
                        }

                        if (Potassium != DPotassium)
                        {
                            ClientSendMsg($"pub fertilize {DPotassium}*");
                            Potassium = DPotassium;
                        }

                        //一天当中特定时间计算平均值
                        hour = DateTime.Now.Hour;
                        minute = DateTime.Now.Minute;
                        second = DateTime.Now.Second;
                        if ((hour == 12) && (minute == 0) && (0 <= second && second <= 10))
                        {
                            double[] recordings = new double[] { 0, 0, 0, 0, 0 };
                            string[] parameters = new string[] { "Temperature", "Humidity", "Oxygen", "TVOC", "LightIntensity" };
                            sql = "select Temperature1,Humidity1,Oxygen1,TVOC1,LightIntensity1 FROM [PlantWallDB].[dbo].[SensorData] where account='admin'";
                            using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                            {
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        recordings[0] = reader.GetDouble(0);
                                        recordings[1] = reader.GetDouble(1);
                                        recordings[2] = reader.GetDouble(2);
                                        recordings[3] = reader.GetDouble(3);
                                        recordings[4] = reader.GetDouble(4);
                                    }
                                }
                            }
                            int dayofweek = Convert.ToInt32(DateTime.Now.DayOfWeek);
                            for (int i = 0; i < 5; i++)
                            {
                                sql = string.Format("update [PlantWallDB].[dbo].[Recording] set {0}={1} where account='admin' and Parameter='{2}'",
                                                    "Recording" + dayofweek, recordings[i], parameters[i]);
                                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        #region 定时启动数据采集设备
                        //if (minute == 30)
                        //{
                        //    sql = "update [PlantWallDB].[dbo].[Instruction] set Start=1 where account='admin'";
                        //    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        //    {
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}
                        //if (minute == 35)
                        //{
                        //    sql = "update [PlantWallDB].[dbo].[Instruction] set Start=1 where account='admin'";
                        //    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        //    {
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 接收服务端发来信息的方法
        /// </summary>
        public static void RecvSMsg()
        {
            //持续监听服务端发来的消息 
            while (true)
            {
                try
                {
                    //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
                    byte[] arrRecvmsg = new byte[1024 * 1024];
                    //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                    int length = SocketClient.Receive(arrRecvmsg);
                    //将套接字获取到的字符数组转换为人可以看懂的字符串  
                    string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "\r\n" + strRevMsg + "\r\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("远程服务器已经中断连接！" + ex.Message + "\r\n");
                    break;
                }
            }
        }

        /// <summary>
        /// 发送字符信息到服务端的方法
        /// </summary>
        /// <param name="sendMsg"></param>
        public static void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组     
            SocketClient.Send(arrClientSendMsg);
        }
    }
}
