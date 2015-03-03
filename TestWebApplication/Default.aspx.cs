using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

namespace TestWebApplication
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            TestWebApplication.WebReference.Service1 t = new WebReference.Service1();
            t.Credentials = new NetworkCredential("sd45", "Newegg@317521", "abs_corp");

            try
            {
                string a = t.RequestOnTrac();
                Response.Write(a);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
    }
}