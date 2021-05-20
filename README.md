# Heapy
Unmanaged memory framework

## Usage
```CSharp
    public struct Test
    {
        public int I1 { get; set; }
        public int I2 { get; set; }
    }

    //Allocates memory for single object
    using var unmanagedObject = Unmanaged<Test>.Alloc();
    unmanagedObject[0] = new Test { I1 = 10, I2 = 20 };

    //Or
    using var unmanagedObject = new Test { I1 = 10, I2 = 20 }.ToUnmanagedHeap();

    //Allocates memory for three objects of type Test
    using var unmanagedObject = Unmanaged<Test>.Alloc(3);
    unmanagedObject[0] = new Test { I1 = 10, I2 = 20 };
    unmanagedObject[1] = new Test { I1 = 11, I2 = 21 };
    unmanagedObject[2] = new Test { I1 = 12, I2 = 22 };
```
