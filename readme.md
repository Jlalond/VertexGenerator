# Vertex Generator 
The vertex generator is a tool to pre-calculate all the positions for a mesh.
This is in order to expose mesh state as a graph entry and allow graph-based semantics between them

# _Why?_
As an experimental alternative to marching cubes. Using a graph based mesh generator
allows hypothetically infinite variability in the mesh.

For the sake of this project, and contributor sanity, this generator
only works with 6 faced cubes with each face having 9 vertices,
8 shared around the edges, and one unshared central vertex

Note: I made these in paint

![alt text](CubeFaces.png "Cubes") 

The Left image is a cube expressed as how it will be stored in a 2d matrix, the right is the mesh data offset from origin
In the above photo, this face can be viewed as the bottom or top face of the cube, viewing from a higher or lower perspective on the Z axis

So for 3d, we will need 3 rows of 2d matrices
![alt text](ZChart.png)
We express all cube data as matrix entries increasing on the Z axis.

# Contributing
Currently I have no concrete vision of where this is going, but feel free to open issues or contribute PR's,
if you believe a feature is critical for this to be viable