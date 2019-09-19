select * from student
order by sno 
offset ((@pageIndex-1)*@pageSize) rows
fetch next @pageSize rows only;


-- 分页查询第2页，每页有10条记录
select * from student
order by sno  
offset 10 rows
fetch next 10 rows only ;

 MSSQL   SELECT SCOPE_IDENTITY() AS
 
  MySQL   ; SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS
  
  PostgreSQL     RETURNING