using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinHeap
{
    private readonly AStarNode[] _elements;
    private int _size;

    public MinHeap(int size)
    {
        _elements = new AStarNode[size];
    }

    private int GetLeftChildIndex(int elementIndex) => 2 * elementIndex + 1;
    private int GetRightChildIndex(int elementIndex) => 2 * elementIndex + 2;
    private int GetParentIndex(int elementIndex) => (elementIndex - 1) / 2;

    private bool HasLeftChild(int elementIndex) => GetLeftChildIndex(elementIndex) < _size;
    private bool HasRightChild(int elementIndex) => GetRightChildIndex(elementIndex) < _size;
    private bool IsRoot(int elementIndex) => elementIndex == 0;

    private AStarNode GetLeftChild(int elementIndex) => _elements[GetLeftChildIndex(elementIndex)];
    private AStarNode GetRightChild(int elementIndex) => _elements[GetRightChildIndex(elementIndex)];
    private AStarNode GetParent(int elementIndex) => _elements[GetParentIndex(elementIndex)];

    private void Swap(int firstIndex, int secondIndex)
    {
        var temp = _elements[firstIndex];
        _elements[firstIndex] = _elements[secondIndex];
        _elements[secondIndex] = temp;
    }

    public bool IsEmpty()
    {
        return _size == 0;
    }

    public AStarNode Peek()
    {
        if (_size == 0)
            throw new IndexOutOfRangeException();

        return _elements[0];
    }

    public AStarNode Pop()
    {
        if (_size == 0)
            throw new IndexOutOfRangeException();

        var result = _elements[0];
        _elements[0] = _elements[_size - 1];
        _size--;

        ReCalculateDown(0);

        return result;
    }

    public void Add(AStarNode element)
    {
        if (_size == _elements.Length)
            throw new IndexOutOfRangeException();

        _elements[_size] = element;
        _size++;

        ReCalculateUp(_size - 1);
    }

	public void Remove(AStarNode element)
	{
        int indexToDelete = checkSubTree(0, element);
		if(indexToDelete != -1)
		{
            _elements[indexToDelete] = _elements[_size - 1];
            _size--;

            int parent = indexToDelete / 2;

            if (indexToDelete == 1 || _elements[parent].getCosts() < _elements[indexToDelete].getCosts())
                ReCalculateDown(indexToDelete);
            else
                ReCalculateUp(indexToDelete);
        }
	}

	public AStarNode ContainsAndGet(AStarNode element)
	{
        int index = checkSubTree(0, element);
        if(index == -1)
        {
            Debug.Log("MinHeap - Can't find obj");
            return null;
        }
        Debug.Log("MinHeap - Found element at pos " + index);
        return _elements[index];
	}

	private int checkSubTree(int index, AStarNode element)
	{
		if(_elements[index].Equals(element))
		{
            return index;
		} else
		{
            int isInRightTree = -1;
            int isInLeftTree = -1;
			if(HasRightChild(index))
			{
                if (GetRightChild(index).getCosts() >= element.getCosts())
				{
                    isInRightTree = checkSubTree(GetRightChildIndex(index), element);
				}
			}
			if(HasLeftChild(index))
			{
				if(GetLeftChild(index).getCosts() >= element.getCosts())
				{
                    isInLeftTree = checkSubTree(GetLeftChildIndex(index), element);
				}
			}
            if(isInLeftTree != -1)
			{
                return isInLeftTree;
			}
			if(isInRightTree != -1)
			{
                return isInRightTree;
			}

            return -1;
		}
	}

	private void ReCalculateDown(int index)
    {
        while (HasLeftChild(index))
        {
            var smallerIndex = GetLeftChildIndex(index);
            if (HasRightChild(index) &&
				GetRightChild(index).getCosts() < GetLeftChild(index).getCosts())
            {
                smallerIndex = GetRightChildIndex(index);
            }

            if (_elements[smallerIndex].getCosts() >= _elements[index].getCosts())
            {
                break;
            }

            Swap(smallerIndex, index);
            index = smallerIndex;
        }
    }

    private void ReCalculateUp(int index)
    {
        while (!IsRoot(index) &&
			_elements[index].getDistanceToGoal() < GetParent(index).getDistanceToGoal())
        {
            var parentIndex = GetParentIndex(index);
            Swap(parentIndex, index);
            index = parentIndex;
        }
    }
}
