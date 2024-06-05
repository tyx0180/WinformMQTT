using HslCommunication.MQTT;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HSLMQTTServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MqttServer mqttServer = new MqttServer();
            mqttServer.ClientVerification += MqttServer_ClientVerification;
            mqttServer.ServerStart(1883);

            // 如果是注册静态方法的接口，则注册类型对象，否则，直接注册变量对象即可
            mqttServer.RegisterMqttRpcApi(typeof(Program));

            //int tick = 0;
            //while (true)
            //{
            //    Thread.Sleep(2000);
            //    // 每秒以字符串的形式推送一个数据给订阅的客户端，1,2,3,4,5,6,7,...
            //    mqttServer.PublishTopicPayload("A", Encoding.UTF8.GetBytes(tick.ToString()));
            //    tick++;   // 让数据变化，方便查看
            //}

            Console.ReadLine();
        }

        [HslMqttApi()]
        public static int Calculate(int a, int b)
        {
            return a * 10 + b * 2;    // 假设这个就是我们的核心的算法
        }

        private static int MqttServer_ClientVerification(MqttSession mqttSession, string clientId, string userName, string passwrod)
        {
            // 返回0 表示验证成功，返回非0 表示失败
            if (userName == "admin" && passwrod == "123456") return 0;
            return 4;


            // 返回错误码说明 Return error code description
            // 1: unacceptable protocol version
            // 2: identifier rejected
            // 3: server unavailable
            // 4: bad user name or password
            // 5: not authorized
        }
    }
}
