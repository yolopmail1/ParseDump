using System.Text;

string[] lines = File.ReadAllLines(@"C:\Logs\backup.ldif");

var utilisateurs = new List<string>();

var objectClass = "";
var description = "";
var uid = "";
var mail = "";
var strCode = "";
var pwd = "";

foreach (string line in lines)
{
    if (line.Contains("dn:"))
    {
        if (!string.IsNullOrEmpty(uid) && uid.Substring(0, 1) == ":")
        {
            uid = decode(uid);
        }

        if (!string.IsNullOrEmpty(description) && description.Substring(0, 1) == ":")
        {
            description = decode(description);
        }

        if (!string.IsNullOrEmpty(mail) && mail.Substring(0, 1) == ":")
        {
            mail = decode(mail);
        }

        if (!string.IsNullOrEmpty(pwd) && pwd.Substring(0, 1) == ":")
        {
            Console.WriteLine(pwd);
            try
            {
                pwd = decode(pwd);
            }
            catch
            {
                pwd = "oups";
            }
            Console.WriteLine(pwd);
        }

        if (objectClass == "inetOrgPerson")
        {
            if (strCode.Contains(","))
            {
                strCode = strCode.Replace(",", "");
            }

            if (pwd != null && !pwd.Contains("Oups"))
            {
                utilisateurs.Add($"{uid};{mail};{strCode};{pwd}");
            }
        }

        objectClass = uid = description = mail = pwd = null;

        if (line.Contains("ou="))
        {
            if (line.Contains("ou=teteDeReseau"))
            {
                strCode = "teteDeReseau";
            }
            else if (line.Contains("ou=ST0"))
            {
                strCode = line.Substring((line.IndexOf("ou=ST0") + "ou=".Length), 8).Trim();
            }
        }
    }
    else
    {
        if (line.Contains("objectClass:"))
        {
            var val = line.Substring(line.IndexOf("objectClass:") + "objectClass:".Length).Trim();
            objectClass = new string[] { "inetOrgPerson" }.Any(a => a == val) ? val : objectClass;
        }

        if (line.Contains("uid:"))
        {
            uid = line.Substring(line.IndexOf("uid:") + "uid:".Length).Trim();
        }

        if (line.Contains("mail:"))
        {
            mail = line.Substring(line.IndexOf("mail:") + "mail:".Length).Trim();
        }

        if (line.Contains("userPassword:"))
        {
            pwd = line.Substring(line.IndexOf("userPassword:") + "userPassword:".Length).Trim();
        }
    }
}



File.WriteAllText(@"C:\Logs\utilisateurs.txt", string.Empty);
using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Logs\", "utilisateurs.txt"), true))
{
    foreach (var uti in utilisateurs)
    {
        outputFile.WriteLine(uti);
    }
}


static string decode(string s)
{
    if (s.Contains(":"))
    {
        s = s.Replace(":", string.Empty);
    }

    s = s.Trim();
    string result = null;

    try
    {
        byte[] data = Convert.FromBase64String(s);
        result = Encoding.Default.GetString(data);
    }
    catch (Exception ex)
    {
        try
        {
            s = s + "=";
            byte[] data = Convert.FromBase64String(s);
            result = Encoding.Default.GetString(data);
        }
        catch
        {
            s = s + "=";
            byte[] data = Convert.FromBase64String(s);
            result = Encoding.Default.GetString(data);
        }

    }

    return result;
}