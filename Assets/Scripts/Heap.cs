using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System;


//T impliments interface IHeapItem
public class Heap<T> where T : IHeapItem<T> 
{
	T[] items;
	int currentItemCount;

	public Heap(int maxHeapSize)
	{
		items = new T[maxHeapSize];
	}

	public void Add(T item)
	{
		item.HeapIndex = currentItemCount;
		items[currentItemCount] = item;                 //added to end of the array, not where it belongs..
        SortUp(item);                                   //compare with parent, swap if it has a higher priority
		currentItemCount++;
	}

	public T removeFirst()
	{
		T firstItem = items [0];
		currentItemCount--;
        //take item at end of the heap and put it at 0
		items [0] = items [currentItemCount];
		items [0].HeapIndex = 0;
        //sorts heap down
		SortDown (items [0]);
		return firstItem;
	}

	public void UpdateItem(T item)
	{
		SortUp (item);
	}

    
	public int Count 
	{
		get {
			return currentItemCount;
		}
	}

    //check if heap contains a specific item
	public bool Contains(T item)
	{
        //return equals method to check if to items are equal
		return Equals (items [item.HeapIndex], item);
	}

	void SortDown(T item)
	{
		while (true) 
		{
            //gets child on the left and right 
			int childIndexLeft = item.HeapIndex * 2 + 1;
			int childIndexRight = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

            //if item has at least one child
			if (childIndexLeft < currentItemCount) {
				swapIndex = childIndexLeft;

				if (childIndexRight < currentItemCount)
                {
                    //which of the two children has a higher priority, if child index left has lower swapindex to right
					if (items [childIndexLeft].CompareTo (items [childIndexRight]) < 0) {
						swapIndex = childIndexRight;
					}
				}

                //compare parent to highest priority child
				if (item.CompareTo (items [swapIndex]) < 0) {
					Swap (item, items [swapIndex]);
				} else {
					return;
				}				
			} else {
				return;
			}
		}
	}


	void SortUp(T item)
	{
        //simple equation to calculate parent of an items index
		int parentIndex = (item.HeapIndex - 1) / 2;

		while(true)
		{
			T parentItem = items[parentIndex];

            //icomparable comes into use, it has a lower f cost if true
			if (item.CompareTo (parentItem) > 0) {
				Swap (item, parentItem);
			} else {
				break;
			}
			parentIndex = (item.HeapIndex - 1) / 2;
		}
	}

    //swap items in the array and heap index values
	void Swap(T itemA, T itemB)
	{
		items [itemA.HeapIndex] = itemB;
		items [itemB.HeapIndex] = itemA;

		int itemAIndex = itemA.HeapIndex;
		itemA.HeapIndex = itemB.HeapIndex;
		itemB.HeapIndex = itemAIndex;
	}
}

public interface IHeapItem<T> : IComparable<T>
{
	int HeapIndex 
	{
		get;
		set;
	}
}