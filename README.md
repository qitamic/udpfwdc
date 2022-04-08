# UDP Forwarder CLI

An implementation of UDP packet forwarder with binding outgoing IP address support.

Usage: `C:\>udpfwdc.exe LocalIP LocalPort RemoteIP RemotePort RemoteBindingIP TimeoutMs [d, daemon]`

E.g. 1: `C:\>udpfwdc.exe 127.0.0.1 53 8.8.8.8 53 0.0.0.0 5000`
Create a localhost DNS "server" which will forward DNS queries from `127.0.0.1` to Google's DNS server `8.8.8.8`.

E.g. 2: `C:\>udpfwdc.exe 127.0.0.1 53 8.8.8.8 53 0.0.0.0 5000 d`
Perform the same as e.g. 1, but with hidden console window

E.g. 3: `C:\>udpfwdc.exe 127.0.0.1 53 9.9.9.9 9953 0.0.0.0 5000`
Perform the same as e.g. 1, but sending queries to Quad9's DNS server at port 9953 instead, to bypass port 53 blocking
