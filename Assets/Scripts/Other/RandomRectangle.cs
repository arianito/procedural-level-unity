using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomRectangle : MonoBehaviour
{
    [SerializeField] private GameObject rectanglePrefab;
    [SerializeField] private Vector2 boundarySize = new Vector2(20f, 20f);
    [SerializeField] private int maxRectangles = 10;

    private void Start()
    {
        List<Rectangle> rectangles = new List<Rectangle>();
        GenerateRandomRectangles(new Rectangle(Vector2.zero, boundarySize.x, boundarySize.y), maxRectangles, rectangles);

        // Instantiate the generated rectangles as GameObjects
        foreach (var rect in rectangles)
        {
            Vector3 position = new Vector3(rect.topLeft.x + rect.width * 0.5f, 0, rect.topLeft.y + rect.height * 0.5f);
            GameObject newRectangle = Instantiate(rectanglePrefab, position, Quaternion.identity);
            newRectangle.transform.localScale = new Vector3(rect.width / 2, 1f, rect.height / 2);
        }
    }

    private void GenerateRandomRectangles(Rectangle boundary, int remainingRectangles, List<Rectangle> rectangles)
    {
        if (remainingRectangles <= 0)
        {
            return;
        }

        // Randomly choose horizontal or vertical splitting
        bool splitHorizontal = Random.Range(0, 2) == 0;

        if (splitHorizontal)
        {
            float splitY = boundary.topLeft.y + Random.Range(0f, boundary.height);
            Rectangle upperRectangle = new Rectangle(boundary.topLeft, boundary.width, splitY - boundary.topLeft.y);
            Rectangle lowerRectangle = new Rectangle(new Vector2(boundary.topLeft.x, splitY), boundary.width, boundary.topLeft.y + boundary.height - splitY);

            GenerateRandomRectangles(upperRectangle, remainingRectangles / 2, rectangles);
            GenerateRandomRectangles(lowerRectangle, remainingRectangles / 2, rectangles);
        }
        else
        {
            float splitX = boundary.topLeft.x + Random.Range(0f, boundary.width);
            Rectangle leftRectangle = new Rectangle(boundary.topLeft, splitX - boundary.topLeft.x, boundary.height);
            Rectangle rightRectangle = new Rectangle(new Vector2(splitX, boundary.topLeft.y), boundary.topLeft.x + boundary.width - splitX, boundary.height);

            GenerateRandomRectangles(leftRectangle, remainingRectangles / 2, rectangles);
            GenerateRandomRectangles(rightRectangle, remainingRectangles / 2, rectangles);
        }

        // Randomly place a rectangle in the current boundary
        float rectWidth = 1f + Random.Range(0f, boundary.width - 1f);
        float rectHeight = 1f + Random.Range(0f, boundary.height - 1f);
        float rectX = boundary.topLeft.x + Random.Range(0f, boundary.width - rectWidth);
        float rectY = boundary.topLeft.y + Random.Range(0f, boundary.height - rectHeight);

        rectangles.Add(new Rectangle(new Vector2(rectX, rectY), rectWidth, rectHeight));
    }

    // 2D Rectangle structure
    private struct Rectangle
    {
        public Vector2 topLeft;
        public float width;
        public float height;

        public Rectangle(Vector2 _topLeft, float _width, float _height)
        {
            topLeft = _topLeft;
            width = _width;
            height = _height;
        }
    }
}