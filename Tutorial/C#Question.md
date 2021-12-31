# Interview Questions

## 1. What is the difference between Checked and CheckState in CheckBox?
### CheckBox.Checked:
    Gets or sets the state of the CheckBox.
### CheckBox.CheckState:
    Gets or set a value indicating whether the CheckBox is in the checked state.

## 2. What is the difference between IEnumerable and List?
### IEnumerable:
    Exposes an enumerator, which supports a simple iteration over a non-generic collection.
    - When you might use an IEnumerable:
        A massive database table, you don’t want to copy the entire thing into memory and cause performance issues in your application.
### List:
    Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.
    - When you might use a List or Array:
        You need the results right away and are possibly mutating the structure you are querying later on.

## 3. What is the difference between Array and List?
### Arrays is a reference Type
    You can store multiple variables of the same type in an array data structure. You declare an array by specifying the type of its elements. If you want the array to store elements of any type, you can specify object as its type. In the unified type system of C#, all types, predefined and user-defined, reference types and value types, inherit directly or indirectly from Object.
    - When you might use an Arrays:
        Use arrays if the size of data is fixed and there is no write and traverse operation required. For example: Parameters to a method. Main function in C# console app is a good example.

### List is a generic type
    - When you might use an List:
        If the size of the data is not very huge, and you need to traverse and write operations on the data use List. Because Lists internally use dynamic arrays, increasing the size of the array at run time is an expensive operations.

## 4. What is the worst time complexity of Arrays and Hashtable?
### The time complexity of Arrays
    The worst time complexity of Arrays is O(n).

### The time complexity of Hash
    Hash tables suffer from O(n) worst time complexity due to two reasons:
    1. If too many elements were hashed into the same key: looking inside this key may take O(n) time.
    2. Once a hash table has passed its load balance - it has to rehash [create a new bigger table, and re-insert each element to the table].
    
#### Average time complexity of different data structures for different operations
|  Data structure   | Access  | Search  | Insertion  | Deletion  |
|  ----  | ----  | ----  | ----  | ----  |
| Array  | O(1) | O(N) | O(N) | O(N) |
| Hash Table  | O(1) | O(1) | O(1) | O(1) |
| Stack  | O(N) | O(N) | O(1) | O(1) |
| Queue  | O(N) | O(N) | O(1) | O(1) |
| Singly Linked list  | O(N) | O(N) | O(1) | O(1) |
| Doubly Linked List  | O(N) | O(N) | O(1) | O(1) |
| Binary Search Tree  | O(log N) | O(log N) | O(log N) | O(log N) |
| AVL Tree  | O(log N) | O(log N) | O(log N) | O(log N) |
| Binary Tree  | O(log N) | O(log N) | O(log N) | O(log N) |
| Red Black Tree  | O(log N) | O(log N) | O(log N) | O(log N) |

#### The Worst time complexity of different data structures for different operations
|  Data structure   | Access  | Search  | Insertion  | Deletion  |
|  ----  | ----  | ----  | ----  | ----  |
| Array  | O(1) | O(N) | O(N) | O(N) |
| Hash Table  | O(N) | O(N) | O(N) | O(N) |
| Stack  | O(N) | O(N) | O(1) | O(1) |
| Queue  | O(N) | O(N) | O(1) | O(1) |
| Singly Linked list  | O(N) | O(N) | O(1) | O(1) |
| Doubly Linked List  | O(N) | O(N) | O(1) | O(1) |
| Binary Search Tree  | O(N) | O(N) | O(N) | O(N) |
| AVL Tree  | O(log N) | O(log N) | O(log N) | O(log N) |
| Binary Tree  | O(N) | O(N) | O(N) | O(N) |
| Red Black Tree  | O(log N) | O(log N) | O(log N) | O(log N) |


## 5. What is the difference Mutex and Semaphore in multi thread?
### Mutex
    Mutex works like a lock in C# for thread synchronization, but it works across multiple processes. Mutex provides a mechnism to prevent two threads from performance one or more actions simultaneously.
 
### Semaphore
    Semaphore allows one or more threads to enter and execute their task with thread safety. Object of semaphore class takes two parameters. First parameter explains the number of processes for initial start and the second parameter is used to define the maximum number of processes which can be used for initial start. The second parameter must be equal or greater than the first parameter.

