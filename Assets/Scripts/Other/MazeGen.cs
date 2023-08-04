using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class Room
{
    public float X;
    public float Y;
    public float Width;
    public float Height;


    public Room(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float Area()
    {
        return Width * Height;
    }

    public float Ratio()
    {
        return Width / Height;
    }

    public void Inset(float amount)
    {
        var xAmount = amount;
        var yAmount = amount;

        if (Width <= xAmount * 2)
        {
            xAmount = Width / 2;
        }

        if (Height <= yAmount * 2)
        {
            yAmount = Height / 2;
        }

        X += xAmount;
        Width -= xAmount * 2;

        Y += yAmount;
        Height -= yAmount * 2;
    }

    public Room[] Split(float minArea, float a)
    {
        if (Area() < minArea)
            return null;

        var rooms = new Room[2];

        var isHorizontal = Width > Height;

        var size = Mathf.Max(Width, Height);

        var s = 8.0f;
        var p = size / s;

        var rnd = Random.Range(0.0f, 1.0f);

        var i = p + rnd * (s - 2) * p;

        if (isHorizontal)
        {
            rooms[0] = new Room(X, Y, i, Height);
            rooms[1] = new Room(X+i, Y, Width - i, Height);
        }
        else
        {
            rooms[0] = new Room(X, Y, Width, i);
            rooms[1] = new Room(X, Y + i, Width, Height - i);
        }


        return rooms;
    }

    public void Draw(Color c, float scale)
    {
        var v0 = new Vector3(X, 0, Y) * scale;
        var v1 = new Vector3(X + Width, 0, Y) * scale;
        var v2 = new Vector3(X + Width, 0, Y + Height) * scale;
        var v3 = new Vector3(X, 0, Y + Height) * scale;


        Debug.DrawLine(v0, v1, c);
        Debug.DrawLine(v1, v2, c);
        Debug.DrawLine(v2, v3, c);
        Debug.DrawLine(v3, v0, c);
    }

    public Vector2 Center()
    {
        return new Vector2(X + Width / 2, Y + Height / 2);
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Width}, {Height}]";
    }
}
public class MazeGen : MonoBehaviour
{
    private List<Room> _rooms = new List<Room>();

    void Start()
    {
        StartCoroutine(Create());
    }

    IEnumerator Create()
    {
        var i = 0;
        while (true)
        {
            
            _rooms.Clear();
            GenerateRooms(new Room(0, 0, 100, 100), 300, i++, ref _rooms);

            _rooms.Sort((a, b) => a.Ratio() > b.Ratio() ? 1 : -1);
            var k = 0;
            _rooms.RemoveAll((r) =>
            {
                r.Inset(1);
                return r.Area() < 40 || k++ > 10;
            });
            yield return new WaitForSeconds(1.0f);
            
        }
    }

    void Update()
    {
        _rooms.ForEach(r => r.Draw(Color.black, 1.0f));
    }

    void GenerateRooms(Room space, float minArea, float i, ref List<Room> arr)
    {
        if (space.Split(minArea, i) is var split && split != null)
        {
            GenerateRooms(split[0], minArea * 1.34f, i + 0.75f, ref arr);
            GenerateRooms(split[1], minArea * 0.86f, i + 0.55f, ref arr);
            return;
        }

        arr.Add(space);
    }
}