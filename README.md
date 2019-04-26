# WinTunnel-plus
Windows TCP Tunnel+ (Bypassing ISP Content Filtering)

First Install the tunnel on client and server. Then install a proxy server on server. On server you should connect tunnel to proxy.

on client you should connect tunnel to tunnel server;

To install service use this:

"WinTunnel.exe -install"


# SAMPLE CONFIG for WinTunnel.ini file
First, create a section, then add two parameter "accept" and "connect".

accept is listening port

connect is address and port of server of tunnel

template for client `WinTunnel.ini` file:
```
[TUNNEL_CLIENT]
accept = 8080

connect = (YOUR TUNNEL SERVER IP ADDRESS):9090
```

template for server `WinTunnel.ini` file:
```
[TUNNEL_SERVER]
accept = 9090

connect = (YOUR PROXY SERVER ADDRESS ON SERVER):8080
```

Your traffic pass through your network as following:

USER --> LOCAL WinTunnel Client -------------------------> Remote WinTunnel Server --> Proxy server --> Internet(Freedom!)

#NOTE
This project is based on Windows TCP Tunnel. You can find it here: http://www.codeproject.com/Articles/14617/Windows-TCP-Tunnel

LICENSE: MIT
