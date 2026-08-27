using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using EntityFramework.BulkInsert.Extensions;
using Repository.Pattern.Core.UnitOfWork;
using IsolationLevel = System.Data.IsolationLevel;
using System.Threading.Tasks;

namespace TrackoApi.Service
{
    public interface IStationeryBookService : IService<StationeryBook>
    {
        void MapBook(StationeryBook book, IUnitOfWorkAsync uow);
        void UpdateBookIssueDate(StationeryBook book, IUnitOfWorkAsync uow);
        void RevokeBookIssue(StationeryBook book, IUnitOfWorkAsync uow);
        void UpdateExpiryDate(long bookId, DateTime? expirydate, IUnitOfWorkAsync uow);
    }
    public class StationeryBookService : Service<StationeryBook>, IStationeryBookService
    {
        private readonly IRepositoryAsync<StationeryBook> _repository;
        public StationeryBookService(IRepositoryAsync<StationeryBook> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public void MapBook(StationeryBook book,IUnitOfWorkAsync uow)
        {
            //if (book.OfficeId.GetValueOrDefault(0)==0)throw new BusinessException(ErrorCode.GLB106,$"Office is required for Mapping a book {book.Name}.");
            if (!book.AllotedDate.HasValue||default(DateTime)==book.AllotedDate.GetValueOrDefault()) throw new BusinessException(ErrorCode.GLB106, $"Alloting Date is required for Mapping a book {book.Name}.");
            if (book.NatureId == 1233)/*Book*/
            {
                var logs = new List<StationeryBookLog>();
                for (int i = 0; i < book.NoOfPages; i++)
                {
                    var page = (book.StartingNumber + i).ToString().PadLeft(book.NoOfDigits, '0');
                    var log = new StationeryBookLog
                    {
                        AllotedDate = book.AllotedDate.Value,
                        BookId = book.Id,
                        ClientId = book.ClientId,
                        IsUsed = false,
                        ObjectState = ObjectState.Added,
                        OfficeId = book.OfficeId,
                        TypeId = book.TypeId,
                        NatureId = book.NatureId,
                        PageNo = $"{book.Prefix}{page}",
                        CreatedDOE = DateTime.Now,
                        ExpiryDate = book.ExpiryDate
                    };
                    logs.Add(log);
                }
                using (var transaction = new TransactionScope())
                {
                    if (book.Id > 0)
                    {
                        _repository.Update(book);
                        book.ObjectState = ObjectState.Modified;                        
                    }
                    else
                    {
                        _repository.Insert(book);
                        book.ObjectState = ObjectState.Added;
                    }                    
                    uow.SaveChanges();
                    logs?.ForEach(x => x.BookId = book.Id);
                    uow.BulkInsert(logs);
                    transaction.Complete();
                }
            }
            if(book.NatureId == 1232/*Auto*/|| book.NatureId == 1624/*Serial*/|| book.NatureId == 1234/*Manual*/)
            {
                var page = book.NatureId != 1234?(book.StartingNumber).ToString().PadLeft(book.NoOfDigits, '0'):"";
                var log = new StationeryBookLog
                {
                    AllotedDate = book.AllotedDate.Value,
                    BookId = book.Id,
                    ClientId = book.ClientId,
                    IsUsed = false,
                    ObjectState = ObjectState.Added,
                    OfficeId = book.OfficeId,
                    TypeId = book.TypeId,
                    NatureId = book.NatureId,
                    PageNo = $"{book.Prefix}{page}",
                    CreatedDOE = DateTime.Now,
                    ExpiryDate = book.ExpiryDate
                };
                _repository.GetRepository<StationeryBookLog>().Insert(log);
                _repository.Update(book);
                book.ObjectState = ObjectState.Modified;
                uow.SaveChanges();
            }
            
        }

        public void UpdateBookIssueDate(StationeryBook book, IUnitOfWorkAsync uow)
        {
            uow.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                if (!book.AllotedDate.HasValue || default(DateTime) == book.AllotedDate.GetValueOrDefault()) throw new BusinessException(ErrorCode.GLB106, $"Alloting Date is required for Mapping a book {book.Name}.");
                book.ObjectState = ObjectState.Modified;
                Update(book);
                uow.SaveChanges();
                uow.Context.Database.ExecuteSqlCommand($"UPDATE [dbo].[mBookLog] SET [AllotedDate]='{book.AllotedDate.Value.ToString("yyyy-MM-dd")}',[OfficeId]={(book.OfficeId.HasValue ? book.OfficeId.ToString() : "NULL")},[ClientId]={(book.ClientId.HasValue ? book.ClientId.ToString() : "NULL")},ExpiryDate={(book.ExpiryDate.HasValue ? "'" + book.ExpiryDate?.ToString("yyyy-MM-dd") + "'" : "NULL")} WHERE [BookId]={book.Id}");
                uow.Commit();
            }
            catch (Exception)
            {
                uow.Rollback();
                throw;
            }
        }
        public void UpdateExpiryDate(long bookId,DateTime? expirydate, IUnitOfWorkAsync uow)
        {
            uow.Context.Database.ExecuteSqlCommand($"UPDATE [dbo].[mBookLog] SET ExpiryDate={(expirydate.HasValue ? "'" + expirydate?.ToString("yyyy-MM-dd") + "'" : "NULL")} WHERE [BookId]={bookId}");
        }
        public void RevokeBookIssue(StationeryBook book, IUnitOfWorkAsync uow)
        {
            uow.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                book.MappingRemark = null;               
                book.IssueToPerson = null;
                book.AllotedDate = null;
                book.ObjectState = ObjectState.Modified;
                Update(book);
                uow.SaveChanges();
                uow.Context.Database.ExecuteSqlCommand($"DELETE FROM [dbo].[mBookLog] WHERE [BookId]={book.Id}");
            }
            catch (Exception)
            {
                uow.Rollback();
                throw;
            }
            uow.Commit();
        }
    }

}
