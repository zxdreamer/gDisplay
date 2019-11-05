#pragma once
#include<string>
#include<mutex>
using namespace std;
class tank {
public:	
	~tank();
	static tank* getObj();
	int getID();
private:
	tank()
	{
		g_id = 10;
	}
	tank(const tank&) = delete;
	tank& operator=(const tank&) = delete;
private:
	static tank *obj;
	int g_id;
	static mutex mx;
};