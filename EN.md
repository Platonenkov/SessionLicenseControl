# SessionLicenseControl

The project is designed to take into account sessions, and control licenses in the software
At startup, the data is read from the encrypted archive and checked according to the requirements.

the library was created with the support of https://github.com/Infarh

### Quick Start::

`Install-SessionLicenseControl -Version 1.0.0`

`Install-Package SessionLicenseControl.WPF -Version 1.0.0 For WPF`

## To work with a license:
### Creating:

```C#
  var lic = new LicenseGenerator(Secret, hdd, expirationDate, Ouner, check_sessions);
  lic.CreateLicenseFile(LicenseFilePath);
```
where:

* `Secret` - secret string to encrypt data
* `hdd` - HDD id of PC where file will use
* `expirationDate` - Date when license will expire
* `Ouner` - for whom the license is created
* `check_sessions` - enable session control in licenses
* `LicenseFilePath` - path, where license file will saved

`You can use "LicenseCreator" to create a license through the console`

![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20generator.gif)

`Or use WPF application`

![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/wpf_license_generator.png)

`To work with a license file:`
```C#
  var controller = new SessionLicenseController(LicenseFilePath, Secret, StartNewSession, "Admin");
```
If the license has expired or the data has been compromised - you will receive an error at this stage

`To view license information:`
```C#
     controller.License.ToString(); //or controller.License.GetLicenseInformation()
```
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20info%20sample.png)

`view sessions data`
```C#
    foreach (var (date_time, sessions) in controller.SessionController.GetSessionData())
    {
        $"Day: {date_time:dd.MM.yyyy}".ConsoleYellow();
        foreach (var session in sessions)
        {
            session.ConsoleRed();
        }
    }
```
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20session%20sample.png)

`how to check for console:`
```C#
var flag = true;
string row = null;
while (flag)
  try
  {
      var controller = row is null ? new SessionLicenseController(SessionsFilePath, Secret, true, "Admin") : new SessionLicenseController(row, Secret, SessionsFilePath, true, "Admin");
      flag = false;
      "License information:".ConsoleYellow();
      controller.License.ToString().ConsoleRed();
  }
  catch (Exception)
  {
      Console.WriteLine("License is bad, Enter license code or add file");
      row = Console.ReadLine();
  }
```

`how to check for WPF:`
```C#
    LicenseChecker.CheckLicense("license.lic", "testwpf", true, "admin");
```
`if there is an error in the WPF license, the interface will be blocked by the license line input window`
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/wpf_license_end.png)
