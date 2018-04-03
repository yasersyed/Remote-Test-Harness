
1) Message Creation
	implement Messages class, that contains -
	Author,
	TimeStamp,
	ToPoint,
	FromPoint,
	and body

2) Queue -
	Single Individual Queue is used for Client, Server and Repository - instead of the sender and receiver queue

	Blocking queue was quite helpful for reducing load to the repository and server, making it easier to implement than the 
	sender and receiver queue.
3) Repository - 
	Repository is a separate server and hence uses download and upload file functions for sending data to client and server
	with the help of File Streaming.
