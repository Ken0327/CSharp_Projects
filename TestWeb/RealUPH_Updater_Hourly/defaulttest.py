from mock import patch,mock,MagicMock
import unittest
import pandas as pd
from pandas.util.testing import assert_frame_equal
import datetime
from DaemonConfig import Config

class TestMethods(unittest.TestCase):
    
    def test_DateRangeByShift(self):
        shift = 0
        thisdate = datetime.datetime(2019,12,26,20,0,0)
        # expect_result = [datetime.datetime(2019, 12, 26, 19, 0),
        #                 datetime.datetime(2019, 12, 26, 18, 0),
        #                 datetime.datetime(2019, 12, 26, 17, 0),
        #                 datetime.datetime(2019, 12, 26, 16, 0),
        #                 datetime.datetime(2019, 12, 26, 15, 0),
        #                 datetime.datetime(2019, 12, 26, 14, 0),
        #                 datetime.datetime(2019, 12, 26, 13, 0),
        #                 datetime.datetime(2019, 12, 26, 12, 0),
        #                 datetime.datetime(2019, 12, 26, 11, 0),
        #                 datetime.datetime(2019, 12, 26, 10, 0),
        #                 datetime.datetime(2019, 12, 26, 9, 0),
        #                 datetime.datetime(2019, 12, 26, 8, 0)]
        expect_result = [datetime.datetime(2019, 12, 26, 20, 0)]
        config = Config()
        func_result = config.GetTimeIndexByShift(shift,thisdate)
        self.assertEqual(expect_result, func_result)


    # def test_CompareModle(self):
    #     t1 = "hello"
    #     t2 = "HELLO"
    #     self.assertEqual(t1.upper(), t2)

    # def test_CheckResult(self):
    #     a1 = 3
    #     a2 = 4
    #     total = 7
    #     sum1 = a1+a2
    #     self.assertEqual(total,sum1)

    # def test_DataFrame(self):
    #     df1 = pd.DataFrame({'a': [1, 2], 'b': [3, 4]})
    #     df2 = pd.DataFrame({'a': [1, 2], 'b': [3.0, 4.0]})
    #     assert_frame_equal(df1, df2)

    # @patch("path.to.file.pandas.read_sql")
    # def test_get_df(read_sql_mock: Mock):
    #     read_sql_mock.return_value = pd.DataFrame({"foo_id": [1, 2, 3]})
    #     results = get_df()
    #     read_sql_mock.assert_called_once()
    #     pd.testing.assert_frame_equal(results, pd.DataFrame({"bar_id": [1, 2, 3]})


if __name__ == '__main__':
    unittest.main()