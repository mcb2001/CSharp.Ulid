# Universally Unique Lexicographically Sortable Identifier

C# .NET Standard 2.0 port of [alizain/ulid](https://github.com/alizain/ulid)

## Usage

```
using CSharp.Ulid;

internal class Program
{
    private static void Main(string[] args)
    {
        Ulid ulidRandom = Ulid.NewUlid();

        Ulid ulidSpecific = new Ulid
        {
            TimeStamp_0 = 3,
            TimeStamp_1 = 4,
            TimeStamp_2 = 5,
            TimeStamp_3 = 6,
            TimeStamp_4 = 7,
            TimeStamp_5 = 8,
            Randomness_0 = 13,
            Randomness_1 = 14,
            Randomness_2 = 15,
            Randomness_3 = 16,
            Randomness_4 = 17,
            Randomness_5 = 18,
            Randomness_6 = 19,
            Randomness_7 = 20,
            Randomness_8 = 21,
            Randomness_9 = 22,
        };

        Ulid ulidFromLongBytes = new Ulid(123456L, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        Ulid ulidFromBytesBytes = new Ulid(new byte[] { 0, 1, 2, 3, 4, 5 }, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        Ulid ulidFromBytes = new Ulid(new byte[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        Ulid.TryParse("00000000000000000000000000", out Ulid ulitParsed);

        Ulid ulidDefault = default(Ulid);

        byte[] byteExplicit = (byte[])new Ulid();

        byte[] byteFromMethod = new Ulid().ToByteArray();

        string stringRepresentation = new Ulid().ToString();

    }
}
```

## Prior Art

- [alizain/ulid](https://github.com/alizain/ulid)
- [RobThree/NUlid](https://github.com/RobThree/NUlid)
- [oklog/ulid](https://github.com/oklog/ulid)
- [lucasschejtman/FSharp.Ulid](https://github.com/lucasschejtman/FSharp.Ulid)
