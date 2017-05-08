netsh http delete urlacl url="http://+:8080/"
netsh advfirewall firewall delete rule name="SimpleWebServer" dir=in protocol=TCP localport=8080
pause