Differences between prototype in the OCD - Project 3 and Project 4

1) Message Creation in Project 3 and Messages in Project 4
	Message creation in project 3 involved sending streams of data in the form of String arrays,
	in Project 4 we have been asked to implement Messages class, that contains -
	Author,
	TimeStamp,
	ToPoint,
	FromPoint,
	and body

	Improvement over Project 3 Idea:
	Which makes it easier to parse and save in case necessary - as shown with server, getting test requests from the repository
	and storing in with author, timestamp keys

2) Queue -
	Single Individual Queue is used for Client, Server and Repository - instead of the sender and receiver queue
	as displayed in Project 3 OCD.

	Improvement over Project 3 Idea:
	Blocking queue was quite helpful for reducing load to the repository and server, making it easier to implement than the 
	sender and receiver queueu mentioned in project 3 which was very impractical.

3) Repository - 
	Repository is a separate server and hence uses download and upload file functions for sending data to client and server
	with the help of File Streaming.