<%@ Application CodeBehind="Global.asax.cs" Inherits="SimpleEchoBot.WebApiApplication" Language="C#" %>
<%@ Import Namespace="log4net" %>
<%@ Import Namespace="System.IO" %>

<script runat="server">
 void Application_Start(object sender, EventArgs e) 
 {
   log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));
 }

 void Application_Error(object sender, EventArgs e)
 {
  ILog log = LogManager.GetLogger("SleepyCore");
  Exception ex = Server.GetLastError();
  log.Debug("++++++++++++++++++++++++++++");
  log.Error("Exception - \n" + ex);
  log.Debug("++++++++++++++++++++++++++++");
 }
</script>
