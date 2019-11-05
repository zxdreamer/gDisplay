//void merge(vector<int>& arr, int L, int M, int R)
//{
//	int LEFT_SIZE = M - L;
//	int RIGHT_SIZE = R - M + 1;
//	vector<int> left(LEFT_SIZE, 0);
//	vector<int> right(RIGHT_SIZE, 0);
//
//	int i;
//	//填充左边的数组
//	for (i = L;i < M;i++)
//		left[i - L] = arr[i];
//	//填充右边数组
//	for (i = M;i <= R;i++)
//		right[i - M] = arr[i];
//}

//int GetNumberOfK(vector<int> data, int k) {
//	int lf = 0;
//	int rt = data.size() - 1;
//	int mid = 0;
//	int cnt = 0;
//	while (lf <= rt)
//	{
//		mid = (lf + rt) / 2;
//		if (data[mid] == k)
//		{
//			int n = mid, m = mid + 1;
//			while (n>lf && data[n] == k)
//			{
//				cnt++;
//				n--;
//			}
//			while (m<rt && data[m] == k)
//			{
//				cnt++;
//				m++;
//			}
//			break;
//		}
//		else if (data[mid]>k)
//			rt = mid - 1;
//		else if (data[mid]<k)
//			lf = mid + 1;
//	}
//
//	return cnt;
//}

