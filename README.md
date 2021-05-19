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

    //Allocates memory for three objects of type Test
    using var unmanagedObject = Unmanaged<Test>.Alloc(3);
```
