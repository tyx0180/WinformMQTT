using HslCommunication.MQTT;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformMQTT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private MqttClient mqttClient = null;
        private void button1_Click(object sender, EventArgs e)
        {
            mqttClient = new MqttClient(new MqttConnectionOptions()
            {
                IpAddress = "127.0.0.1",
                Port = 1883,
                Credentials = new MqttCredential("admin", "123456")  // 增加用户名密码确认
            });

            mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;

            // 连接成功触发的事件里进行订阅，在connectserver之前绑定事件(断网后再联网会重新订阅主题)
            //mqttClient.OnClientConnected += (MqttClient client) =>
            //{
            //    // 订阅
            //    OperateResult sub = client.SubscribeMessage("A");
            //    if (sub.IsSuccess)
            //    {
            //        // 订阅成功
            //    }
            //    else
            //    {
            //        // 订阅失败
            //    }
            //};

            OperateResult connect = mqttClient.ConnectServer();
            if (!connect.IsSuccess)
            {
                Console.WriteLine("Connect failed: " + connect.Message);
                Console.ReadLine();
                return;
            }
            else {
                Console.WriteLine("连接成功 ");
            }
        }

        private void MqttClient_OnMqttMessageReceived(MqttClient client, MqttApplicationMessage message)
        {
            // 接收到服务器发送的数据触发的事件，这里就是先显示下
            Console.WriteLine(DateTime.Now.ToString() + $" Topic[{message.Topic}] {Encoding.UTF8.GetString(message.Payload)}");

            this.Invoke(new Action(() =>
            {
                lblnum.Text = $" Topic[{message.Topic}] {Encoding.UTF8.GetString(message.Payload)}";
            }));
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (mqttClient != null) {
                // 订阅
                OperateResult sub = mqttClient.SubscribeMessage("A");
                if (sub.IsSuccess)
                {
                    Console.WriteLine("订阅成功");
                }
                else
                {
                    Console.WriteLine("订阅失败");
                }


            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            int val = random.Next(10, 10000);

            MqttApplicationMessage mqttApplicationMessage = new MqttApplicationMessage();
            mqttApplicationMessage.Topic = "A";
            mqttApplicationMessage.Payload = Encoding.UTF8.GetBytes(val.ToString());
            // 如果是实时数据，适合用这个
            mqttApplicationMessage.QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce;    
            OperateResult connect = mqttClient.PublishMessage(mqttApplicationMessage);
            if (connect.IsSuccess)
            {
                // 发布成功
                Console.WriteLine("发布成功");
            }
            else
            {
                // 发布失败
                Console.WriteLine("发布失败");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mqttClient == null) return;
            FormMain formMain = new FormMain(mqttClient);
            formMain.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MqttSyncClient rpc = new MqttSyncClient(new MqttConnectionOptions()
            {
                IpAddress = "127.0.0.1",
                Port = 1883,
                Credentials = new MqttCredential("admin", "123456"),
                UseRSAProvider = true,           // 通信过程加密，防止数据被中间截取
            });
            OperateResult connect = rpc.ConnectServer();
            if (!connect.IsSuccess)
            {
                Console.WriteLine("Connect failed: " + connect.Message);
                return;
            }
            OperateResult<int> read = rpc.ReadRpc<int>("Program/Calculate", new { a = 1, b = 2 });
            if (read.IsSuccess)
            {
                Console.WriteLine("Result: " + read.Content);   // 显示结果
            }
            else
            {
                Console.WriteLine("Called failed: " + read.Message);
            }

            // 在系统退出的时候，调用关闭
            rpc.ConnectClose();
        }
    }
}
