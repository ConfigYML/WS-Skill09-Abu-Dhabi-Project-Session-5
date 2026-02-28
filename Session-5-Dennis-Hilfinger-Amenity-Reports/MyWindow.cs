using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Session_5_Dennis_Hilfinger
{
    class MyWindow : Window
    {
        public MyWindow(AppShell shell)
        {
            base.Page = shell;
        }

        /*
        protected override void OnDestroying()
        {
            using (var db = new AirlineContext())
            {
                var login = db.Logins.Where(l => l.LogoutTime == null).OrderByDescending(l => l.LoginTime).FirstOrDefault();
                if (login != null)
                {
                    login.LogoutTime = DateTime.UtcNow;
                    db.Update(login);
                }
                db.SaveChanges();
            }
            base.OnDestroying();
        }*/
    }
}
