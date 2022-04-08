using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

public class UdpFwd
{
	//var
	private IPEndPoint epLocal;
	private IPEndPoint epRemote;
	private IPEndPoint epBind;
	private int iTimeoutMs;

	//ctor ss
	public UdpFwd(IPEndPoint epLocal, IPEndPoint epRemote, IPEndPoint epBind, int iTimeoutMs, out Exception exError)
	{
		exError = null;
		try
		{
			this.epLocal = epLocal;
			this.epRemote = epRemote;
			this.epBind = epBind;
			this.iTimeoutMs = iTimeoutMs;

			dicClientList = new Dictionary<IPEndPoint, UdpClient>();
			udpLocal = new UdpClient(epLocal);
			SuppressConnReset(udpLocal);

			wLetterbox = new Worker<IPEndPoint>(100);
			wLetterbox.Work += wLetterbox_Work;

			wRegister = new Worker<IPEndPoint, byte[]>(100);
			wRegister.Work += wRegister_Work;

			wTable = new Worker();
			wTable.Work += wTable_Work;
			wTable.Do();
		}
		catch (Exception ex)
		{
			exError = ex;
		}
	}
	public void Stop()
	{
		if (udpLocal != null) udpLocal.Close();

		if (dicClientList != null)
		{
			List<IPEndPoint> client = new List<IPEndPoint>(dicClientList.Keys);
			for (int i = 0; i < client.Count; i++)
				dicClientList[client[i]].Close();
		}
	}

	//client manage
	object oClient = new object();
	private Dictionary<IPEndPoint, UdpClient> dicClientList;
	private void ClientAdd(IPEndPoint ep)
	{
		lock (oClient)
		{
			dicClientList.Add(ep, null);
		}
	}
	private void ClientRemove(IPEndPoint ep)
	{
		lock (oClient)
		{
			dicClientList.Remove(ep);
		}

	}
	private bool ClientContains(IPEndPoint ep)
	{
		lock (oClient)
		{
			return dicClientList.ContainsKey(ep);
		}
	}

	//receive client
	private UdpClient udpLocal;
	private Worker wTable;
	private void wTable_Work()
	{
		while (true)
		{
			try
			{
				IPEndPoint ep = null;
				byte[] data = udpLocal.Receive(ref ep);
				wRegister.Do(ep, data);
			}
			catch (SocketException ex1)
			{
				Console.WriteLine(ex1.SocketErrorCode);
				if (ex1.SocketErrorCode == SocketError.Interrupted)
					return;
				else if (Debugger.IsAttached)
					Debugger.Break();
			}
			catch (Exception ex2)
			{
				if (Debugger.IsAttached)
					Debugger.Break();
			}
		}
	}

	//process client query
	private Worker<IPEndPoint, byte[]> wRegister;
	private void wRegister_Work(IPEndPoint ep, byte[] data)
	{
		bool bNewClient = !ClientContains(ep);

		UdpClient udp;

		if (bNewClient)
		{
			udp = new UdpClient(epBind);
			SuppressConnReset(udp);
			udp.Client.ReceiveTimeout = iTimeoutMs;

			ClientAdd(ep);
			dicClientList[ep] = udp;
		}
		else
			udp = dicClientList[ep];

		udp.Send(data, data.Length, epRemote);

		if (bNewClient)
			wLetterbox.Do(ep);
	}

	//process client answer
	private Worker<IPEndPoint> wLetterbox;
	private void wLetterbox_Work(IPEndPoint ep)
	{
		UdpClient udp = dicClientList[ep];
		IPEndPoint epDummy = null;
		while (true)
		{
			try
			{
				byte[] data = udp.Receive(ref epDummy);
				udpLocal.Send(data, data.Length, ep);
			}
			catch (SocketException ex1)
			{
				if (ex1.SocketErrorCode == SocketError.TimedOut)
				{
					udp.Close();
					ClientRemove(ep);
					return;
				}
				else if (Debugger.IsAttached)
					Debugger.Break();
			}
			catch (Exception ex2)
			{
				if (Debugger.IsAttached)
					Debugger.Break();
			}
		}
	}

	//helper
	private void SuppressConnReset(UdpClient udp)
	{
		int SIO_UDP_CONNRESET = -1744830452;
		udp.Client.IOControl(
			(IOControlCode)SIO_UDP_CONNRESET,
			new byte[] { 0, 0, 0, 0 },
			null
		);
	}

}
