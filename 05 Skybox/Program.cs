﻿using Engine;

namespace Skybox
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("5 Skybox", false, 1600, 900, true, 0, 0))
#else
            using (Game cl = new Game("5 Skybox", true, 0, 0, true, 0, 0))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene<TestScene3D>();

                cl.Run();
            }
        }
    }
}
