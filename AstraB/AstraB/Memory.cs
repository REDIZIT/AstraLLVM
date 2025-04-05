namespace AVM;

public class Memory
{
    // public int basePointer;
    // public int stackPointer;
    // public int heapPointer;
    
    // public readonly int stackSize = 512;

    private List<MemoryChunk> chunks = new();
    public MemoryLogger logger;

    public string Log => logger.ToString();

    public Memory()
    {
        logger = new(this);
        
        chunks.Add(MemoryChunk.Regular(0, 1024, logger));

        // basePointer = stackPointer = 0;
        // heapPointer = stackSize;
    }

    // public int Allocate_Stack(int bytesToAllocate, int stackPointer)
    // {
    //     int pointer = stackPointer;
    //
    //     logger.Log_Allocate(bytesToAllocate);
    //     
    //     stackPointer += bytesToAllocate;
    //     // if (stackPointer >= stackSize)
    //     // {
    //     //     throw new Exception($"StackOverFlow stack pointer = {stackPointer}, stackSize = {stackSize}");
    //     // }
    //     
    //     return pointer;
    // }

    // public int Allocate_Heap(int bytesToAllocate)
    // {
    //     logger.Log_AllocateHeap(bytesToAllocate);
    //     
    //     int pointer = heapPointer;
    //     heapPointer += bytesToAllocate;
    //     return pointer;
    // }

    // public void Deallocate_Stack(int bytesToDeallocate)
    // {
    //     logger.Log_Deallocate(bytesToDeallocate);
    //     stackPointer -= bytesToDeallocate;
    // }

    public void Write(int address, byte value)
    {
        MemoryChunk chunk = GetChunk(address);
        chunk.Write(chunk.ToLocal(address), value);
    }

    public void WriteShort(int address, short value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void WriteInt(int address, int value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void WriteLong(int address, long value)
    {
        Write(address, BitConverter.GetBytes(value));
    }
    public void Write(int address, byte[] value, bool noLogs = false)
    { 
        MemoryChunk chunk = GetChunk(address);
        chunk.Write(chunk.ToLocal(address), value, noLogs);
    }

    public byte Read(int address)
    {
        MemoryChunk chunk = GetChunk(address);
        return chunk.Read(chunk.ToLocal(address));
    }
    public short ReadShort(int address)
    {
        MemoryChunk chunk = GetChunk(address);
        return chunk.ReadShort(chunk.ToLocal(address));
    }
    public int ReadInt(int address)
    {
        MemoryChunk chunk = GetChunk(address);
        return chunk.ReadInt(chunk.ToLocal(address));
    }
    public long ReadLong(int address)
    {
        MemoryChunk chunk = GetChunk(address);
        return chunk.ReadLong(chunk.ToLocal(address));
    }

    public byte[] Read(int address, byte sizeInBytes)
    {
        MemoryChunk chunk = GetChunk(address);
        return chunk.Read(chunk.ToLocal(address), sizeInBytes);
    }

    // public void PushInt(int value)
    // {
    //     Push(BitConverter.GetBytes(value));
    // }
    // public void Push(byte[] bytes)
    // {
    //     logger.Log_Push(bytes);
    //     
    //     Write(stackPointer, bytes, true);
    //     stackPointer += bytes.Length;
    // }

    // public int PopInt()
    // {
    //     return BitConverter.ToInt32(Pop(sizeof(int)));
    // }
    // public byte[] Pop(byte bytesToPop)
    // {
    //     logger.Log_Pop(bytesToPop);
    //     
    //     stackPointer -= bytesToPop;
    //     return Read(stackPointer, bytesToPop);
    // }


    // public int ToAbs(int rbpOffset)
    // {
    //     return basePointer + rbpOffset;
    // }

    public void Copy(int fromAddress, int toAddress, byte size)
    {
        MemoryChunk fromChunk = GetChunk(fromAddress);
        byte[] bytes = fromChunk.Read(fromAddress, size);
        
        MemoryChunk toChunk = GetChunk(toAddress);
        toChunk.Write(toAddress, bytes);
    }

    // public void Dump(string filepath)
    // {
    //     File.WriteAllBytes(filepath, bytes);
    // }

    private MemoryChunk GetChunk(int address)
    {
        foreach (MemoryChunk chunk in chunks)
        {
            if (chunk.address <= address && address < chunk.address + chunk.size)
            {
                return chunk;
            }
        }

        if (0xB8000 <= address && address < 0xB8000 + 80 * 25 * 2)
        {
            MemoryChunk chunk = MemoryChunk.VGA(logger);
            chunks.Add(chunk);
            return chunk;
        }

        throw new Exception($"Memory chunk was not found for address {address}");
    }
}