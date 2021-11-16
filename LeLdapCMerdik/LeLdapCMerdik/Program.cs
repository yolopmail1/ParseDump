using System.Text;

string[] lines = File.ReadAllLines(@"C:\Logs\backup.ldif");

var structures = new List<string>();
var utilisateurs = new List<string>();
var groupes = new List<string>();
var members = new List<string>();
var groupeMembers = new List<Tuple<string, List<string>>>();

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
            pwd = decode(pwd);
        }

        if (objectClass == "organizationalUnit" && uid != null && uid.Contains("ST0"))
        {
            structures.Add($"{uid};{description}");
        }
        else if (objectClass == "inetOrgPerson")
        {
            if (strCode.Contains(","))
            {
                strCode = strCode.Replace(",", "");
            }

            utilisateurs.Add($"{uid};{mail};{strCode},{pwd}");
        }
        else if (objectClass == "groupOfNames")
        {
            strCode = strCode.Replace(",", "");
            groupes.Add($"{uid};{description};{strCode}");
            groupeMembers.Add(new Tuple<string, List<string>>(uid, members));
        }

        objectClass = uid = description = mail = pwd = null;
        members = new List<string>();

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
            objectClass = new string[] { "organizationalUnit", "inetOrgPerson", "groupOfNames" }.Any(a => a == val) ? val : objectClass;
        }

        if (line.Contains("ou:"))
        {
            uid = line.Substring(line.IndexOf("ou:") + "ou:".Length).Trim();
        }

        if (line.Contains("uid:"))
        {
            uid = line.Substring(line.IndexOf("uid:") + "uid:".Length).Trim();
        }

        if (line.Contains("cn:") && string.IsNullOrEmpty(uid))
        {
            uid = line.Substring(line.IndexOf("cn:") + "cn:".Length).Trim();
        }

        if (line.Contains("mail:"))
        {
            mail = line.Substring(line.IndexOf("mail:") + "mail:".Length).Trim();
        }

        if (line.Contains("userPassword:"))
        {
            pwd = line.Substring(line.IndexOf("userPassword:") + "userPassword:".Length).Trim();
        }

        if (line.Contains("description:"))
        {
            description = line.Substring(line.IndexOf("description:") + "description:".Length).Trim();
        }

        if (line.Contains("member") && objectClass == "groupOfNames")
        {

            var val = line.Substring(line.IndexOf("member:") + "member:".Length).Trim();

            if (!val.Contains("SPG") && !val.Contains("Y249") && !val.Contains("STG") && !val.Contains("nobody") && !val.Contains("cn=") && !val.Contains("uid=STU000000"))
            {
                if (val.Contains("uid="))
                {
                    val = val.Replace("uid=", "");
                }

                members.Add(line.Substring(line.IndexOf("member:") + "member:".Length).Trim());
            }
        }
    }
}


File.WriteAllText(@"C:\Logs\structures.txt", string.Empty);
using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Logs\", "structures.txt"), true))
{
    foreach (var str in structures)
    {
        outputFile.WriteLine(str);
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


File.WriteAllText(@"C:\Logs\groupes.txt", string.Empty);
using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Logs\", "groupes.txt"), true))
{
    foreach (var grp in groupes)
    {
        outputFile.WriteLine(grp);
    }
}

File.WriteAllText(@"C:\Logs\groupMembers.txt", string.Empty);
using (StreamWriter outputFile = new StreamWriter(Path.Combine(@"C:\Logs\", "groupMembers.txt"), true))
{
    foreach (var grp in groupeMembers)
    {
        foreach (var member in grp.Item2)
        {
            outputFile.WriteLine(grp.Item1 + "," + member);
        }
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