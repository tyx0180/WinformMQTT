using HslCommunication;
using HslCommunication.MQTT;
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
    public partial class FormMain : Form
    {
        private MqttClient mqttClient;
        public FormMain(MqttClient client)
        {
            InitializeComponent();
            mqttClient = client;
            Subscrib();
        }

        // 窗体初始化的时候订阅
        public void Subscrib()
        {
            OperateResult send = mqttClient.SubscribeMessage("A");

            if (!send.IsSuccess)
            {
                Console.WriteLine("SubscribeMessage Failed:" + send.Message);
            }
            else
            {
                // 获取订阅的信息，绑定本类的触发事件
                SubscribeTopic subscribeTopic = mqttClient.GetSubscribeTopic("A");
                if (subscribeTopic != null) {
                    mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
                }
                    
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

        // 在窗体结束的时候，调用下面的方法
        public void UnSubscrib()
        {
            // 取消订阅操作，需要先解绑注册事件
            SubscribeTopic subscribeTopic = mqttClient.GetSubscribeTopic("A");
            if (subscribeTopic != null)
            {
                subscribeTopic.OnMqttMessageReceived -= MqttClient_OnMqttMessageReceived;

                // 取消订阅，为了不影响其他的界面的订阅信息（可能其他子界面也有订阅相同的主题），这里先判断订阅计数，减到 0 才真正的取消订阅
                // 如果你的主题只有一个窗体使用到的话，那么这里就不需要判断，直接取消订阅即可
                if (subscribeTopic.RemoveSubscribeTick() <= 0)
                {
                    OperateResult send = mqttClient.UnSubscribeMessage("A");
                    if (!send.IsSuccess)
                        Console.WriteLine("UnSubscribeMessage Failed:" + send.Message);
                }
            }
        }
    }
}
