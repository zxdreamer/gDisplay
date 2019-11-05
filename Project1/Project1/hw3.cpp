#include <iostream>
#include <vector>
#include <algorithm>
#include <string>
#include <map>
#include <string>
#include <sstream>
#include <set>
#include <stack>

using namespace std;

int main03()
{
	string str;
	getline(cin, str);
	vector<char> vtopt;
	stack<char> skopt;
	stack<char> sksym;
	int len = str.size();
	for (int i = 0;i < len;i++)
	{
		if (str[i] == '0' || str[i] == '1')
			vtopt.push_back(str[i]);
		else if (str[i] == '(')
			sksym.push(str[i]);
		else if (str[i] == '!')
		{
			while (sksym.size() > 0)
			{
				char c = sksym.top();
				if (c == '!')
				{
					vtopt.push_back(c);
					sksym.pop();
				}
				else
					break;
			}
			sksym.push(str[i]);
		}
		else if (str[i] == '&')
		{
			while (sksym.size() > 0)
			{
				char c = sksym.top();
				if (c == '!' || c == '&')
				{
					vtopt.push_back(c);
					sksym.pop();
				}
				else
					break;
			}
			sksym.push(str[i]);
		}
		else if (str[i] == '|')
		{
			while (sksym.size() > 0)
			{
				char c = sksym.top();
				if (c == '!' || c == '&' || c == '|')
				{
					vtopt.push_back(c);
					sksym.pop();
				}
				else
					break;
			}
			sksym.push(str[i]);
		}
		else if (str[i] == ')')
		{
			while (sksym.size() > 0 && sksym.top() != '(')
			{
				char c = sksym.top();
				vtopt.push_back(c);
				sksym.pop();
			}
			if (sksym.top() == '(')
				sksym.pop();
		}
	}

	while (sksym.size() > 0)
	{
		char c = sksym.top();
		vtopt.push_back(c);
		sksym.pop();
	}
	int len2 = vtopt.size();
	for (int i = 0;i < len2;i++)
	{
		if (vtopt[i] == '0' || vtopt[i] == '1')
			skopt.push(vtopt[i]);
		else if (vtopt[i] == '!')
		{
			char c = skopt.top() == '1' ? '0' : '1';
			skopt.pop();
			skopt.push(c);
		}
		else
		{
			int a = skopt.top() - '0';
			skopt.pop();
			int b = skopt.top() - '0';
			skopt.pop();
			char c;
			if (vtopt[i] == '&')
				c = (char)(a&b) + '0';
			else if (vtopt[i] == '|')
				c = (char)(a | b) + '0';

			skopt.push(c);
		}
	}
	cout << skopt.top() << endl;
	while (1);
	return 0;
}