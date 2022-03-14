using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using System.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public PollDto GetPollBL(int pollId)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.PollID == pollId);

                if (poll != null)
                {
                    return new PollDto()
                    {
                        PollID = poll.PollID,
                        AddedByUserID = poll.AddedByUserID,
                        PollTypeID = poll.PollTypeID,
                        CreateUpdateDate = poll.CreateUpdateDate,
                        AreaIdentifier = poll.AreaIdentifier,
                        UiAreaIdentifier = poll.UiAreaIdentifier,
                        QuestionText = poll.QuestionText,
                        IsCurrent = poll.IsCurrent,
                        IsArchived = poll.IsArchived,
                        ArchivedDate = Convert.ToDateTime(poll.ArchivedDate),
                        PollOptions = GetPollOptionsBL(poll.PollID)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetPollBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreatePollBL(string question, string areaIdentifier, string uiAreaIdentifier, int addedByUserId, int pollTypeId)
        {
            try
            {
                var poll = new EntityFramework.Poll();

                poll.AddedByUserID = addedByUserId;
                poll.PollTypeID = pollTypeId;
                poll.CreateUpdateDate = DateTime.Now;
                poll.AreaIdentifier = areaIdentifier;
                poll.UiAreaIdentifier = uiAreaIdentifier;
                poll.QuestionText = question;
                poll.IsCurrent = false;
                poll.IsArchived = false;

                entityFramework.Polls.AddObject(poll);
                entityFramework.SaveChanges();

                return poll.PollID;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.CreatePollBL(): " + ex.ToString());
                return -1;
            }
        }


        public bool UpdatePollBL(int pollId, string questionText, bool isCurrent, bool isArchived)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.PollID == pollId);

                if (poll != null)
                {
                    /* Case: The poll is current, but is being set to not current. */
                    if (poll.IsCurrent == true && isCurrent == false)
                    {
                        /* Setting a current poll to not current. */
                        poll.IsCurrent = false;
                    }

                    /* Case: The poll is not current, but is being set to current. */
                    if (poll.IsCurrent == false && isCurrent == true)
                    {
                        /* An eventual other current poll in the same area, is being set to not current. */
                        SetCurrentPollNotCurrent(poll.AreaIdentifier);

                        poll.IsCurrent = true;
                    }

                    /* Case: The poll is archived, but is being set not to archived. */
                    if (poll.IsArchived == true && isArchived == false)
                    {
                        poll.IsArchived = false;
                        poll.ArchivedDate = null;
                    }

                    /* Case: The poll is not archived, but is being set to archived. */
                    if (poll.IsArchived == false && isArchived == true)
                    {
                        poll.IsArchived = true;
                        poll.IsCurrent = false;
                        poll.ArchivedDate = DateTime.Now;
                    }

                    /* Case: The QuestionText has been modified. */
                    if (poll.QuestionText != questionText)
                    {
                        poll.QuestionText = questionText;
                        poll.CreateUpdateDate = DateTime.Now;
                    }

                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.UpdatePollBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeletePollBL(int pollId)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.PollID == pollId);

                if (poll != null)
                {
                    /* Deleting the PollOptions of the Poll. */
                    var pollOptionsToDelete = entityFramework.PollOptions.Where(p => p.PollID == pollId);
                    pollOptionsToDelete.ToList().ForEach(p => entityFramework.DeleteObject(p));

                    /* Deleting the PollUsers rows connected to the Poll (The registered Users who have voted in the Poll) */
                    var pollUserRowsToDelete = entityFramework.PollUsers.Where(p => p.PollID == pollId);
                    pollUserRowsToDelete.ToList().ForEach(p => entityFramework.DeleteObject(p));

                    /* Deleting the Poll */
                    entityFramework.Polls.DeleteObject(poll);
                    entityFramework.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.DeletePollBL(): " + ex.ToString());
                return false;
            }
        }


        public bool SetPollCurrentBL(int pollId, string areaIdentifier)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.PollID == pollId);

                /* Getting an eventual other current poll for the same area, in order to set it not current anymore, so that a new poll can be set to current. */
                SetCurrentPollNotCurrent(poll.AreaIdentifier);

                if (poll != null)
                {
                    poll.IsCurrent = true;
                    entityFramework.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.SetPollCurrentBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Getting an eventual other current poll for the same area, in order to set it not current anymore, so that a new poll can be set to current.
        /// </summary>
        /// <param name="areaIdentifier"></param>
        private void SetCurrentPollNotCurrent(string areaIdentifier)
        {
            try
            {
                var currentPoll = entityFramework.Polls.SingleOrDefault(p => p.AreaIdentifier == areaIdentifier & p.IsCurrent == true);

                if (currentPoll != null)
                {
                    /* Setting a current poll to not current. */
                    currentPoll.IsCurrent = false;
                    entityFramework.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.SetCurrentPollNotCurrent(): " + ex.ToString());
            }
        }


        public bool SetPollArchivedBL(int pollId)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.PollID == pollId);

                if (poll != null)
                {
                    poll.IsArchived = true;
                    poll.IsCurrent = false;
                    poll.ArchivedDate = DateTime.Now;
                    entityFramework.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.SetPollArchivedBL(): " + ex.ToString());
                return false;
            }
        }


        public PollDto GetCurrentCommunityPollBL(int communityId, int userId)
        {
            try
            {
                var areaIdentifier = ConvertIdIntToAreaIdentifier(communityId);

                var poll = entityFramework.Polls.SingleOrDefault(p => p.AreaIdentifier == areaIdentifier & p.IsCurrent == true);

                if (poll != null)
                {
                    return new PollDto()
                    {
                        PollID = poll.PollID,
                        AddedByUserID = poll.AddedByUserID,
                        PollTypeID = poll.PollTypeID,
                        CreateUpdateDate = poll.CreateUpdateDate,
                        AreaIdentifier = poll.AreaIdentifier,
                        QuestionText = poll.QuestionText,
                        IsCurrent = poll.IsCurrent,
                        IsArchived = poll.IsArchived,
                        ArchivedDate = Convert.ToDateTime(poll.ArchivedDate),
                        HasUserVoted = HasUserVotedInCommunityPollBL(userId, poll.PollID),
                        PollOptions = GetPollOptionsBL(poll.PollID)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetCurrentCommunityPollBL(): " + ex.ToString());
                return null;
            }
        }


        public bool HasUserVotedInCommunityPollBL(int userId, int pollId)
        {
            try
            {
                var pollUser = entityFramework.PollUsers.Where(p => p.UserID == userId && p.PollID == pollId).SingleOrDefault();

                if (pollUser != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.HasUserVotedInCommunityPollBL(): " + ex.ToString());
                return false;
            }
        }


        public PollDto GetCurrentAnonymousPollBL(string areaIdentifier)
        {
            try
            {
                var poll = entityFramework.Polls.SingleOrDefault(p => p.AreaIdentifier == areaIdentifier && p.IsCurrent == true);

                if (poll != null)
                {
                    return new PollDto()
                    {
                        PollID = poll.PollID,
                        AddedByUserID = poll.AddedByUserID,
                        PollTypeID = poll.PollTypeID,
                        CreateUpdateDate = poll.CreateUpdateDate,
                        AreaIdentifier = poll.AreaIdentifier,
                        UiAreaIdentifier = poll.UiAreaIdentifier,
                        QuestionText = poll.QuestionText,
                        IsCurrent = poll.IsCurrent,
                        IsArchived = poll.IsArchived,
                        ArchivedDate = Convert.ToDateTime(poll.ArchivedDate),
                        PollOptions = GetPollOptionsBL(poll.PollID)
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetCurrentAnonymousPollBL(): " + ex.ToString());
                return null;
            }
        }





        public PollDto VoteCommunityPollBL(int pollOptionId, int userId)
        {
            try
            {
                var pollOption = entityFramework.PollOptions.SingleOrDefault(p => p.PollOptionID == pollOptionId);

                /* Adding the vote for the PollOption. */
                if (pollOption != null)
                {
                    pollOption.Votes++;
                    entityFramework.SaveChanges();
                }

                /* Getting the PollID for the PollOption. */
                var pollId = pollOption.PollID;

                /* Register that that this particular user has voted in this particular poll. */
                var pollUserRegistration = new EntityFramework.PollUser();
                pollUserRegistration.PollID = pollId;
                pollUserRegistration.UserID = userId;
                entityFramework.PollUsers.AddObject(pollUserRegistration);
                entityFramework.SaveChanges();

                /* Returning the poll with the new status. */
                return GetPollBL(pollId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.VoteCommunityPollBL(): " + ex.ToString());
                return null;
            }            
        }


        public PollDto VoteAnonymousPollBL(int pollOptionId)
        {
            try
            {
                var pollOption = entityFramework.PollOptions.SingleOrDefault(p => p.PollOptionID == pollOptionId);

                /* Adding the vote for the PollOption. */
                if (pollOption != null)
                {
                    pollOption.Votes++;
                    entityFramework.SaveChanges();
                }

                /* Getting the PollID for the PollOption. */
                var pollId = pollOption.PollID;

                /* Returning the poll with the new status. */
                return GetPollBL(pollId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.VoteAnonymousPollBL(): " + ex.ToString());
                return null;
            }
        }


        public List<PollOptionDto> GetPollOptionsBL(int pollId)
        {
            try
            {
                var pollOptions = entityFramework.PollOptions.Where(p => p.PollID == pollId);

                var pollOptionDtos = new List<PollOptionDto>();

                foreach (var p in pollOptions)
                {
                    pollOptionDtos.Add
                    (
                        new PollOptionDto()
                        {
                            PollOptionID = p.PollOptionID,
                            PollID = p.PollID,
                            AddedByUserID = p.AddedByUserID,
                            CreateUpdateDate = p.CreateUpdateDate,
                            OptionText = p.OptionText,
                            Votes = p.Votes
                        }
                    );
                }
                return pollOptionDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetPollOptionsBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreatePollOptionBL(int pollId, int addedByUserId, string optionText)
        {
            try
            {
                var pollOption = new EntityFramework.PollOption();

                pollOption.PollID = pollId;
                pollOption.AddedByUserID = addedByUserId;
                pollOption.CreateUpdateDate = DateTime.Now;
                pollOption.OptionText = optionText;
                pollOption.Votes = 0;

                entityFramework.PollOptions.AddObject(pollOption);
                entityFramework.SaveChanges();

                return pollOption.PollOptionID;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.CreatePollOptionBL(): " + ex.ToString());
                return -1;
            }
        }


        public bool UpdatePollOptionBL(int pollOptionId, string optionText)
        {
            try
            {
                var pollOption = entityFramework.PollOptions.SingleOrDefault(p => p.PollOptionID == pollOptionId);

                if (pollOption != null)
                {
                    pollOption.OptionText = optionText;
                    pollOption.CreateUpdateDate = DateTime.Now;

                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.UpdatePollOptionBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeletePollOptionBL(int pollOptionId)
        {
            try
            {
                var pollOption = entityFramework.PollOptions.SingleOrDefault(p => p.PollOptionID == pollOptionId);

                if (pollOption != null)
                {
                    entityFramework.PollOptions.DeleteObject(pollOption);
                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.DeletePollOptionBL(): " + ex.ToString());
                return false;
            }
        }


        public List<PollDto> GetPollsForCommunityBL(int communityId, int userId, bool isArchived, int pageSize, int pageNumber)
        {
            try
            {
                var areaIdentifier = ConvertIdIntToAreaIdentifier(communityId);

                var polls = entityFramework.Polls.Where(p => p.AreaIdentifier == areaIdentifier & p.IsArchived == isArchived);

                var pollDtos = new List<PollDto>();

                if (polls != null)
                {
                    /* Probing for whether there will be a next page of data beyond the current page. */
                    var numberOfPolls = polls.Count();
                    bool hasNextPageOfData = numberOfPolls > (pageNumber * pageSize) ? true : false;
                    /* Ordering by CreateUpdateDate */
                    polls = polls.OrderByDescending(p => p.CreateUpdateDate);
                    /* Pagination */
                    polls = polls.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                    foreach (var p in polls)
                    {
                        var pollDto = new PollDto()
                        {
                            PollID = p.PollID,
                            AddedByUserID = p.AddedByUserID,
                            PollTypeID = p.PollTypeID,
                            CreateUpdateDate = p.CreateUpdateDate,
                            AreaIdentifier = p.AreaIdentifier,
                            QuestionText = p.QuestionText,
                            IsCurrent = p.IsCurrent,
                            IsArchived = p.IsArchived,
                            HasUserVoted = HasUserVotedInCommunityPollBL(userId, p.PollID),
                            PollOptions = GetPollOptionsBL(p.PollID),
                            HasNextPageOfData = hasNextPageOfData
                        };
                        pollDtos.Add(pollDto);
                    }
                    return pollDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetPollsForCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets anonymous Polls for different kind of areas: e.g. "Country", "Sjaelland", "Hilleroed", "2700"
        /// </summary>
        /// <param name="areaIdentifier">a string which uniquely identifies an area, e.g. "Country" ( The whole of Denmark)
        /// ""Sjaelland" (Region), "Hilleroed" (Municipality), "2700" (PostalCode)</param>
        public List<PollDto> GetPollsForAreaBL(string areaIdentifier, bool isArchived, int pageSize, int pageNumber)
        {
            try
            {
                var polls = entityFramework.Polls.Where(p => p.AreaIdentifier == areaIdentifier & p.IsArchived == isArchived);

                var pollDtos = new List<PollDto>();

                if (polls != null)
                {
                    /* Probing for whether there will be a next page of data beyond the current page. */
                    var numberOfPolls = polls.Count();
                    bool hasNextPageOfData = numberOfPolls > (pageNumber * pageSize) ? true : false;
                    /* Ordering by CreateUpdateDate */
                    polls = polls.OrderByDescending(p => p.CreateUpdateDate);
                    /* Pagination */
                    polls = polls.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                    foreach (var p in polls)
                    {
                        var pollDto = new PollDto()
                        {
                            PollID = p.PollID,
                            AddedByUserID = p.AddedByUserID,
                            PollTypeID = p.PollTypeID,
                            CreateUpdateDate = p.CreateUpdateDate,
                            AreaIdentifier = p.AreaIdentifier,
                            UiAreaIdentifier = p.UiAreaIdentifier,
                            QuestionText = p.QuestionText,
                            IsCurrent = p.IsCurrent,
                            IsArchived = p.IsArchived,
                            PollOptions = GetPollOptionsBL(p.PollID),
                            HasNextPageOfData = hasNextPageOfData
                        };
                        pollDtos.Add(pollDto);
                    }
                    return pollDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetPollsForAreaBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Returns the 4 different types of anonymous Polls, Users in the Editor Role can create: 
        /// Country, Region, Municipality and Postal Code.
        /// </summary>
        public List<AnonymousPollTypeDto> GetAnonymousPollTypesBL()
        {
            try
            {
                var types = entityFramework.PollTypes;

                if (types != null)
                {
                    var typeDtos = new List<AnonymousPollTypeDto>();

                    foreach (var t in types)
                    {
                        /* We only get the 4 types of anonymous Polls, namely: 
                         1. Country, 2. Region, 3. Municipality, 4. Postal Code,
                         thus we are excluding the fifth Poll which requires authentication:
                         5. Community*/
                        if (t.PollTypeID != 5)
                        {
                            typeDtos.Add
                            (
                                new AnonymousPollTypeDto()
                                {
                                    AnonymousPollTypeID = t.PollTypeID,
                                    Name = t.Name
                                }
                            );
                        }
                    }
                    return typeDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PollBL.GetAnonymousPollTypesBL: " + ex.ToString());
                return null;
            }
        }


        //public List<PollDto> GetArchivedPollsForCommunityBL(string areaIdentifier)
        //{
        //    try
        //    {
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.TraceError("Problem in BusinessLogic.GetArchivedPollsForCommunityBL(): " + ex.ToString());
        //        return null;
        //    }
        //}
    }
}