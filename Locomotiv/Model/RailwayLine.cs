using System;
using System.Security.Cryptography;
public class RailwayLine
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Color { get; set; }

    public List<Block> Blocks { get; set; }
}