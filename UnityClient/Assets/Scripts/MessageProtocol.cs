using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
public sealed class MessageProtocol
{
    public const int HEADLENGTH = 5;  // 协议首部长度(命令1字节+参数1字节+数据长度4字节=6字节)
    private byte _Command = 0;//128 worlddata 129login 130logout 131updateuser 132updatechunk 133updatechunkinternal 134chunkgen 135returnalluserdata 136loginreturn
    //138emitparticle 139clientmodifychunk 140updatealluser 141clientupdateuser 
    public byte Command { get => this._Command; }  // 协议信令标志 (值范围0~255)
                                                   // private byte _Param = 0;
                                                   // public byte Param { get => this._Param; }  // 信令参数标志(范围0~255)
    private int _DataLength = 0;
    public int DataLength { get => this._DataLength; }  // 有效载荷数据长度
    private byte[] _MessageData = new byte[0];
    public byte[] MessageData { get => this._MessageData; }  // 完整消息体
    private byte[] _MoreData = new byte[0];
    public byte[] MoreData { get => this._MoreData; }  // 完整消息体多余的数据
    public MessageProtocol(byte[] buffer)  // 通过字节数组创建消息协议对象(相当于拆包过程)
    {
        if (buffer == null || buffer.Length < HEADLENGTH)  // 当要拆包的数据长度小于协议首部长度,就不进行拆包,因为根本无法得出需要的信息,比如有效数据长度
        {
            return;
        }
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader br = new BinaryReader(ms);
        try
        {
            this._Command = br.ReadByte();
            // this._Param = br.ReadByte();
            this._DataLength = br.ReadInt32();
            if (buffer.Length - HEADLENGTH >= this._DataLength)  // 如果要拆包的数据长度减去协议首部长度大于等于需要的数据长度就可以提取出实际的数据
            {
                this._MessageData = br.ReadBytes(this._DataLength);  // 读取DataLength长度的数据
            }
            if (buffer.Length - HEADLENGTH - this.DataLength > 0)  // 如果要拆包的数据长度减去协议头长度减去实际数据的长度大于0就说明还有多余数据
            {
                this._MoreData = br.ReadBytes(buffer.Length - HEADLENGTH - this._DataLength);  // 读取多余数据
            }
        }
        catch (Exception)
        {
            // 异常处理
        }
        br.Close();
        ms.Close();
    }
    public MessageProtocol(byte command, byte[] messageData)  // 通过传入协议参数构造一个协议对象
    {
        this._Command = command;
        //  this._Param = param;
        this._MessageData = messageData;
        this._DataLength = messageData.Length;
    }
    public byte[] GetBytes()  // 将协议对象转换成二进制数据包(相当于封包过程)
    {
        try
        {
            byte[] bytes = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(this._Command);
            //   bw.Write(this._Param);
            bw.Write(this._DataLength);
            bw.Write(this._MessageData);
            bytes = ms.ToArray();
            bw.Close();
            ms.Close();
            return (bytes);
        }
        catch (Exception)
        {
            return (new byte[0]);
        }
    }
    public (byte Command, int DataLength, byte[] MessageData) GetMessage()  // 获取完整的一条消息(忽略多余字节部分)
    {
        (byte Command, int DataLength, byte[] MessageData) returnValue = (this._Command, this._MessageData.Length, this._MessageData);
        return (returnValue);
    }
    public static byte[] CombineBytes(byte[] firstBytes, int firstIndex, int firstLength, byte[] secondBytes, int secondIndex, int secondLength)
    {
        byte[] bytes = null;
        MemoryStream ms = new MemoryStream();
        ms.Write(firstBytes, firstIndex, firstLength);
        ms.Write(secondBytes, secondIndex, secondLength);
        bytes = ms.ToArray();
        ms.Close();
        return (bytes);
    }
    public static (byte Command, int DataLength) GetHeadInfo(byte[] buffer)  // 读取协议头部分
    {
        (byte Command, int DataLength) returnValue = (0, 0);
        if (buffer == null || buffer.Length < HEADLENGTH)  // 如果数据长度小于协议头根本无法读出完整的协议头内容
        {
            return (returnValue);
        }
        returnValue.Command = buffer[0];

        returnValue.DataLength = BitConverter.ToInt32(buffer, 1);
        return (returnValue);
    }
}