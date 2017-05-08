netsh http add urlacl url="http://+:8080/" user=everyone
netsh advfirewall firewall add rule name="SimpleWebServer" dir=in action=allow protocol=TCP localport=8080
pause