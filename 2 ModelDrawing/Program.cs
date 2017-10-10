﻿using Engine;

namespace ModelDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("2 ModelDrawing", false, 800, 450, true, 0, 4))
#else
            using (Game cl = new Game("2 ModelDrawing", true, 0, 0, true, 0, 4))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif
                
                cl.AddScene(new TestScene(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
