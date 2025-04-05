namespace AVM;

public class MemoryChunk
{
    public int address;
    public int size;
    
    public byte[] bytes;

    public MemoryLogger logger;

    public static MemoryChunk Regular(int address, int size, MemoryLogger logger)
    {
        return new MemoryChunk()
        {
            address = address,
            size = size,
            bytes = new byte[size],
            logger = logger
        };
    }

    public static MemoryChunk VGA(MemoryLogger logger)
    {
        return Regular(0xB8000, 80 * 25 * 2, logger);
    }

    public int ToLocal(int absAddress)
    {
        return absAddress - address;
    }
    
    public void Write(int address, byte value)
    {
        if (address < 0 || address >= bytes.Length)
        {
            throw new Exception($"Write at {address} out of memory bounds ({bytes.Length})");
        }

        logger.Log_Write(address, value);
        
        bytes[address] = value;
    }

    public void Write(int address, byte[] value, bool noLogs = false)
    {
        if (address < 0 || address + value.Length >= bytes.Length)
        {
            throw new Exception($"Write at {address}..{address + value.Length} out of memory bounds ({bytes.Length})");
        }
        
        if (!noLogs) logger.Log_Write(address, value);
        
        for (int i = 0; i < value.Length; i++)
        {
            bytes[address + i] = value[i];
        }
    }

    public byte Read(int address)
    {
        if (address < 0 || address >= bytes.Length)
        {
            throw new Exception($"Read at {address} out of memory bounds ({bytes.Length})");
        }
        return bytes[address];
    }
    public short ReadShort(int address)
    {
        return BitConverter.ToInt16(bytes, address);
    }
    public int ReadInt(int address)
    {
        return BitConverter.ToInt32(bytes, address);
    }
    public long ReadLong(int address)
    {
        return BitConverter.ToInt64(bytes, address);
    }

    public byte[] Read(int address, byte sizeInBytes)
    {
        if (address < 0 || address + sizeInBytes >= bytes.Length)
        {
            throw new Exception($"Read at {address}..{address + sizeInBytes} out of memory bounds ({bytes.Length})");
        }
        
        byte[] value = new byte[sizeInBytes];
        for (int i = 0; i < sizeInBytes; i++)
        {
            value[i] = bytes[address + i];
        }
        return value;
    }
}