﻿namespace Network.ServerModules
{
    public interface IModule
    {
        void Initialize();
        void Update();
        void Shutdown();
    }
}
