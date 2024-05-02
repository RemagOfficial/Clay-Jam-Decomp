internal interface IGALogger
{
	void log(string data);

	void logStartRequest(string url);

	void logSuccessfulRequest(string url);

	void logFailedRequest(string url, string error);
}
