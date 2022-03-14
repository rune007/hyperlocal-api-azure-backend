using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;
using Microsoft.WindowsAzure.Diagnostics;
using System.Collections;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public CommentDto CreateCommentBL(int newsItemId, int userId, string commentBody)
        {
            try
            {
                var entity = azureTableDataSource.CreateComment(newsItemId, userId, commentBody);

                if (entity != null)
                {
                    var commentDto = new CommentDto()
                    {
                        PartitionKey = entity.PartitionKey,
                        NewsItemID = ConvertCommentPartionKeyToNewsItemId(entity.PartitionKey),
                        RowKey = entity.RowKey,
                        PostedByUserID = entity.UserID,
                        PostedByUserName = GetUserName(entity.UserID),
                        CommentBody = entity.CommentBody,
                        CreateDate = entity.CreateDate,
                        ThumbnailBlobUri = GetUserPhotoThumbnail(entity.UserID),
                        MediumSizeBlobUri = GetUserPhotoMedium(entity.UserID)
                    };

                    /* Incrementing the NumberOfComments on the NewsItem. */
                    entityFramework.procIncrementNumberOfComments(newsItemId);

                    /* Sending out alerts on the newly posted Comment. */
                    AlertUsersOnCommentPosted(commentDto);

                    return commentDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommentBL.CreateCommentBL(): " + ex.ToString());
                return null;
            }
        }


        public bool DeleteCommentBL(int newsItemId, string rowKey)
        {
            try
            {
                var partitionKey = ConvertNewsItemIdToCommentPartionKey(newsItemId);
                azureTableDataSource.DeleteComment(partitionKey, rowKey);

                /* Decrementing the NumberOfComments on the NewsItem. */
                entityFramework.procDecrementNumberOfComments(newsItemId);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommentBL.DeleteCommentBL(): " + ex.ToString());
                return false;
            }
        }


        public List<CommentDto> GetCommentsOnNewsItemBL(int newsItemId, int pageSize, int pageNumber)
        {
            try
            {
                var entities = azureTableDataSource.GetCommentsOnNewsItem(newsItemId, pageSize, pageNumber);

                var commentDtos = new List<CommentDto>();

                if (entities != null)
                {
                    foreach (var e in entities)
                    {
                        commentDtos.Add
                        (
                            new CommentDto()
                            {
                                PartitionKey = e.PartitionKey,
                                RowKey = e.RowKey,
                                PostedByUserID = e.UserID,
                                PostedByUserName = GetUserName(e.UserID),
                                CommentBody = e.CommentBody,
                                CreateDate = e.CreateDate,
                                ThumbnailBlobUri = GetUserPhotoThumbnail(e.UserID),
                                MediumSizeBlobUri = GetUserPhotoMedium(e.UserID)
                            }
                        );
                    }
                    /* In the below lines I am probing for whether there will be a next page of data with Count().
                     Then I am doing pagination with Skip() and Take(). These operations should really have been
                     done against the table entities themselves in class HlTableDataSource.cs, but it turned out
                     difficult to use these linq operators against the table entities, therefore I am doing it 
                     against the CommentDto List instead. */

                    /* Probing for whether there will be a next page of data beyond the current page. */
                    var numberOfComments = commentDtos.Count();
                    bool hasNextPageOfData = numberOfComments > (pageNumber * pageSize) ? true : false;
                    /* Updating the HasNextPageOfData field in CommentDto. */
                    foreach (var c in commentDtos)
                    {
                        c.HasNextPageOfData = hasNextPageOfData;
                    }

                    /* Converting to Queryable in order to do the pagination. */
                    var commentDtosQueryable = commentDtos.AsQueryable();
                    commentDtosQueryable = commentDtosQueryable.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    commentDtos = commentDtosQueryable.ToList();

                    return commentDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommentBL.GetCommentsOnNewsItemBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Deletes the Comments on a NewsItem.
        /// </summary>
        public void DeleteCommentsOnNewsItem(int newsItemId)
        {
            try
            {
                azureTableDataSource.DeleteCommentsOnNewsItem(newsItemId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommentBL.DeleteCommentsOnNewsItem(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Deletes the Comments posted by a particular User.
        /// </summary>
        private void DeleteCommentsPostedByUser(int userId)
        {
            try
            {
                azureTableDataSource.DeleteCommentsPostedByUser(userId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommentBL.DeleteCommentsPostedByUser(): " + ex.ToString());
            }
        }
    }
}