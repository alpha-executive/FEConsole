﻿var AppLang = {
    lang: "中文",
    getFormatString: function (format, params) {
        if (params == null || params.length == 0) {
            return format;
        }

        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof params[number] != 'undefined'
                   ? params[number]
                   : match;
        });
    },
    COMMON_EDIT_SAVE_SUCCESS: "更改成功!",
    COMMON_EDIT_SAVE_FAILED: "更改失败: ",
    COMMON_DELETE_FAILED: "删除失败: ",
    COMMON_DLG_TITLE_ERROR: "错误",
    COMMON_DLG_TITLE_WARN: "警告",
    INDEX_YOY_STATICS_REPORT: "YOY 统计报表",
    INDEX_SUBTXT_PBD: "文章, 日记和书籍",
    INDEX_AVG_PROGRESS_REPORT: "平均进度报表",
    INDEX_SUBTXT_TT: "目标和任务",
    INDEX_REPORT_NAME_AVG: "平均值",
    INDEX_SUBTXT_IMG: "图片",
    INDEX_REPORT_NAME_POST: "文章",
    INDEX_REPORT_NAME_BOOK: "书籍",
    INDEX_REPORT_NAME_DIARY: "日记",
    INDEX_REPORT_NAME_IMGS: "图片",
    INDEX_REPORT_NAME_TARGET: "目标",
    INDEX_REPORT_NAME_TASK: "任务",
    INDEX_PROVIDER_MESSAGE_FMT: "你有{0}条新消息",
    INDEX_EVENT_COUNTER_MSG_FMT: "你有{0}个以上新事件",
    INDEX_TASK_FMT: "你至少有{0}个任务",
    BOOK_INDEX_ROOT_FOLDER: "根目录",
    BOOK_INDEX_NEW_FOLDER: "新建目录",
    BOOK_INDEX_NEW_DOCUMENT: "新建文档",
    BOOK_BTN_NEW_CATEGORY: "新建类别",
    BOOK_LBL_NEW_BOOK: "新建书籍",
    BOOK_LBL_NEWCATEGORY: "新建类别",
    ACCOUNT_FIELD_NEW_REC: "新建账户记录",
    IMAGEMGR_STD_LIST: "列表",
    IMAGEMGR_SLD_SHOW: "幻灯片",
    IMAGEMGR_WATER_FLOW: "流线布局",
    IMAGEMGR_NEW_ALBUM: "新建相册",
    IMAGEMGR_NEW_IMAGE: "新建图片",
    IMAGEMGR_UPLOAD_ERR_MSG: "{1}张中的{0}张图片没有上传成功",
    IMAGEMGR_VIEW_LOAD_ERROR: "加载图片出错！",
    POST_NEW_POST: "新建文章",
    POST_NEW_GROUP: "新建分组",
    DIARY_WEATHER_SUNNY: "晴",
    DIARY_WEATHER_CLOUDY: "多云",
    DIARY_WEATHER_CLOUDY_GUSTS: "多云大风",
    DIARY_WEATHER_CLOUDY_WINDY: "多云微风",
    DIARY_WEATHER_FOG: "大雾",
    DIARY_WEATHER_RAIN: "雨",
    DIARY_WEATHER_WINDY: "微风",
    DIARY_WEATHER_STORM: "暴风雨",
    DIARY_PREFIX_DIARY: "日记 ",
    PROFILE_CHANGE_FAILED: "更新失败: {0}",
    USERMGR_PASS_SAVE_DONE: "密码更新完成!",
    DATA_CUST_FIELD_FILE: "文件上传字段名称",
    DATA_CUST_FIELD_FILE_KEY: "文件上传字段编码",
    DATA_CUST_FIELD_OBJ_REF: "表数据引用字段名称",
    DATA_CUST_FIELD_OBJ_REF_KEY: "表数据引用字段编码",
    DATA_CUST_FIELD_SL: "单选字段名称",
    DATA_CUST_FIELD_SL_KEY: "单选字段编码",
    DATA_CUST_FIELD_STRING: "字符串字段名称",
    DATA_CUST_FIELD_STRING_KEY: "字符串字段编码",
    DATA_CUST_FIELD_INTEGER: "整数字段名称",
    DATA_CUST_FIELD_INTEGER_KEY: "整数字段编码",
    DATA_CUST_FIELD_LONG: "长整数字段名称",
    DATA_CUST_FIELD_LONG_KEY: "长整数字段编码",
    DATA_CUST_FIELD_DATETIME: "时间字段名称",
    DATA_CUST_FIELD_DATETIME_KEY: "时间字段编码",
    DATA_CUST_FIELD_NUMBER: "数字字段名称",
    DATA_CUST_FIELD_NUMBER_KEY: "数字字段编码",
    DATA_CUST_FILE_TYPE: "文件",
    DATA_CUST_OBJREF_TYPE: "引用",
    DATA_CUST_SL_TYPE: "单选",
    DATA_CUST_STRING_TYPE: "字符",
    DATA_CUST_INTEGER_TYPE: "整数",
    DATA_CUST_LONG_TYPE: "长整数",
    DATA_CUST_DATETIME_TYPE: "时间",
    DATA_CUST_NUMBER_TYPE: "数字",
    DATA_CUST_UNKNOWN_TYPE: "未知"
}