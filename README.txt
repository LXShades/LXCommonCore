# LX Common Core
Free stuff for Unity! Use it, steal it, eat it, paint it green! Do whatever you want with it!  

This is where I put the code that I've used across several projects. It isn't worth rewriting each time, and my copy-and-paste buttons are screaming 'there must be a better way!'.  

This contains the code and tools that I wish came with Unity by default, but alas here we are. It's not tailored to any specific individual's needs, but it is absolutely free to use. I don't take requests - note that the only reason this is public is to support my public projects, such as Ringslingers - but bug and security reports are welcomed.  

## Features
Very likely a mish-mash of things you've already seen before, but written by myself and free of licensing burdens.  

* Debug shape draws with line thickness and persistence (+ extra shapes for Gizmos)
* Vector extensions, such as `vector.Horizontal()` for getting the horizontal part of a vector easily. No more `new Vector3(vector.x, 0, vector.z)`.
* WhenReady<> construct, useful for projects where you await access to an object that might not exist yet (e.g. multiplayer projects)
* Time quantization tools, useful for time-sensitive e.g. multiplayer tasks

### Under development
* Component inspector tools