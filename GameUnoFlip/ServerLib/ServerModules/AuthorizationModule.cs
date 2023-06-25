using Microsoft.EntityFrameworkCore;
using Network;
using System.Data.Entity;
using System.Drawing;
using System.Security.Cryptography;

namespace ServerLib.ServerModules
{
    public class AuthorizationModule : IModule
    {
        private NetworkModule? networkModule;
        private DBModule? DBM;
        private Dictionary<Client, t_User>? clientsOnAuth;

        public void Initialize()
        {
            Console.WriteLine($"[Authorization Module] Запуск инициализация");

            DBM = ModuleManager.GetModule<DBModule>();

            clientsOnAuth = new Dictionary<Client, t_User>();

            networkModule = ModuleManager.GetModule<NetworkModule>();
            networkModule.OnClientReciveMessage += NetworkModule_onClientReciveMessage;
            networkModule.OnClientConnected += NetworkModule_onClientConnected;
            networkModule.OnClientDisconnected += NetworkModule_onClientDisconnected;

            Console.WriteLine($"[Authorization Module] Инициализация завершена");
        }

        public void Update()
        {

        }

        public void Shutdown()
        {

        }

        private void NetworkModule_onClientDisconnected(Client client)
        {
            clientsOnAuth?.Remove(client);
        }

        private void NetworkModule_onClientConnected(Client client)
        {
            clientsOnAuth.Add(client, new t_User() { Id = -1 });
        }

        private void NetworkModule_onClientReciveMessage(Client client, Packet packet)
        {
            if (packet.Get<string>(Property.TargetModule) != "AuthorizationModule") return;

            Auth auth;
            t_User? user;
            List<t_User> users;

            switch (packet.Get<AuthMethods>(Property.Method))
            {
                case AuthMethods.register:
                    {
                        auth = packet.Get<Auth>(Property.Data);
                        client.ConnectedID = Register(client, auth.Login, auth.Password);
                    }
                    break;

                case AuthMethods.login:
                    {
                        auth = packet.Get<Auth>(Property.Data);
                        client.ConnectedID = Login(client, auth.Login, auth.Password);
                    }
                    break;

                case AuthMethods.login_OnUseToken:
                    {
                        auth = packet.Get<Auth>(Property.Data);
                        users = DBM.AppDBContext.Users.ToList();

                        user = users.Find(u => u.Login == auth.Login && u.Tokken == auth.Tokken);
                        if (user != null)
                        {
                            Console.WriteLine($"[AuthorizationModule] Клиент {user.Login} успешно авторизован с адреса {client.RemoteEndPoint()}");

                            client.Send(new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, "AuthorizationModule")
                                .Add(Property.Method, AuthMethods.login_OnUseToken)
                                .Add(Property.Data, true));
                        }
                        else
                        {
                            client.Send(new Packet()
                                .Add(Property.Type, PacketType.Response)
                                .Add(Property.TargetModule, "AuthorizationModule")
                                .Add(Property.Method, AuthMethods.login_Error)
                                .Add(Property.Data, false));
                        }
                    }
                    break;
            }

            auth = null;
            user = null;
            users = null;
        }

        private int Register(Client client, string login, string password)
        {
            var users = DBM.AppDBContext.Users.ToList();
            var user = users.Find(u => u.Id == client.ConnectedID);

            if (user != null)
            {
                user.Login = login;
                user.Password = password;
                DBM.AppDBContext.SaveChanges();

                Console.WriteLine($"[AuthorizationModule] Клиент {user.Login} успешно зарегистрирован с адреса {client.RemoteEndPoint()}");

                clientsOnAuth[client] = user;

                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.TargetModule, "AuthorizationModule")
                    .Add(Property.Method, AuthMethods.register_Ok)
                    .Add(Property.Data, new Auth() { Id = user.Id, Login = user.Login, Tokken = user.Tokken }));

                return user.Id;

            }
            else
            {
                return -1;
            }
        }


        private int Login(Client client, string login, string password)
        {
            var users = DBM.AppDBContext.Users.ToList();

            var user = users.Find(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                Console.WriteLine($"[AuthorizationModule] Клиент {user.Login} успешно авторизован с адреса {client.RemoteEndPoint()}");

                var userTemp = users.Find(u => u.Id == client.ConnectedID);
                if (userTemp != null && userTemp.Login.Contains("Guest"))
                {
                    DBM.AppDBContext.Users.Remove(userTemp);
                    DBM.AppDBContext.SaveChanges();
                }

                clientsOnAuth[client] = user;

                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.TargetModule, "AuthorizationModule")
                    .Add(Property.Method, AuthMethods.login_Ok)
                    .Add(Property.Data, new Auth() { Id = user.Id, Login = user.Login, Tokken = user.Tokken }));
                return user.Id;
            }
            else
            {
                return FastReg(client);
            }
        }


        public int FastAuth(Client client, string tokken)
        {
            var users = DBM.AppDBContext.Users.ToList();

            var user = users.Find(u => u.Tokken == tokken);
            if (user != null)
            {
                Console.WriteLine($"[AuthorizationModule] Клиент {user.Login} успешно авторизован с адреса {client.RemoteEndPoint()}");

                clientsOnAuth[client] = user;

                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Method, "FastAuth")
                    .Add(Property.Data, new Auth() { Id = user.Id, Login = user.Login, Tokken = user.Tokken }));
                return user.Id;
            }
            else
            {
                return FastReg(client);
            }
        }

        public int FastReg(Client client)
        {
            var users = DBM.AppDBContext.Users.ToList();
            int count = 0;

            string newLogin = "Guest-" + client.RemoteEndPoint().ToString().Split(":")[0].Replace(".", "");


            while (true)
            {
                if (users.Where(x => x.Login == newLogin + count).Count() == 0) break;
                count++;
            }

            var user = new t_User()
            {
                Login = newLogin + count
            };

            if (users.Where(x => x.Login == user.Login).Count() == 0)
            {
                Console.WriteLine($"[AuthorizationModule] Клиент {user.Login} успешно зарегистрирован с адреса {client.RemoteEndPoint()}");

                user.Tokken = GenerateToken();
                DBM.AppDBContext.Users.Add(user);
                DBM.AppDBContext.SaveChanges();

                clientsOnAuth[client] = user;

                client.Send(new Packet()
                    .Add(Property.Type, PacketType.Connect)
                    .Add(Property.Method, "FastReg")
                    .Add(Property.Data, new Auth() { Id = user.Id, Login = user.Login, Tokken = user.Tokken }));

                return user.Id;

            }
            else
            {
                return -1;
            }
        }

        public static string GenerateToken()
        {
            byte[] token = new byte[32];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(token);
            }

            return Convert.ToBase64String(token);
        }
    }
}
