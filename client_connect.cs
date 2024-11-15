using Godot;
using System.Net.Sockets;
using Moffat.EndlessOnline.SDK.Data;
using System.Linq;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System.Collections.Generic;
using Version = Moffat.EndlessOnline.SDK.Protocol.Net.Version;

public partial class client_connect : Node2D
{
    private TcpClient _client;
    private string _serverAddress = "127.0.0.1";
    private int _serverPort = 8078;

    public override void _Ready()
    {
        _client = new TcpClient();
        _client.Connect(_serverAddress, _serverPort);
        GD.Print("Connected to server.");

        var initPacket = new InitInitClientPacket
        {
            Version = new Version { Major = 0, Minor = 0, Patch = 28 },
            Challenge = 12345,
            Hdid = "161726351"
        };

        Send(initPacket);
    }

    private void Send(InitInitClientPacket packet)
    {
        var writer = new EoWriter();
        packet.Serialize(writer);
        byte[] buf = writer.ToByteArray();

        var data = new List<byte>(buf);
        data.Insert(0, 0xFF);
        data.Insert(0, 0xFF);

        byte[] temp = data.ToArray();
        byte[] lengthBytes = NumberEncoder.EncodeNumber(temp.Length);
        byte[] payload = new byte[] { lengthBytes[0], lengthBytes[1] }.Concat(temp).ToArray();

        _client.Client.Send(payload);
        GD.Print($"Packet sent: Action={packet.Action}, Family={packet.Family}, Length={payload.Length}.");
    }

    public override void _ExitTree()
    {
        if (_client != null && _client.Connected)
        {
            _client.Close();
        }
    }
}