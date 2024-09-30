const express = require('express');
const fs = require('fs');       //fs 모듈 추가
const router = express.Router();

//초기 지원 설정
const initalResources = 
{
    meta : 500,
    crystal : 300,
    deuterirum : 100,
}

//글로벌 플레이어 객체 초기화
global.players = {};            //글로벌 객체 초기화



module.exports = router;        //라우터 내보내기