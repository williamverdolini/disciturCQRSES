﻿angular.module('disc.lesson')
    /*-------------------------------------------------------------------------------
    Vantaggi del DTO:
    - disaccoppiamento tra i dati restituite dal BE e quelli gestiti dal FE
    - presenza di un (Client) Object Model distinto dal (Server( Object e/o Entity Model
    - possibilità di verificare il reale contenuto delle classi a codice (non a runtime)

    riferimenti: http://www.bennadel.com/blog/2527-Defining-Instantiatable-Classes-In-The-AngularJS-Dependency-Injection-Framework.htm
    ---------------------------------------------------------------------------------*/
    .factory('LessonDTO', function () {
        function LessonDTO() {
            this.lessonId = null;
            this.title = null;
            this.discipline = null;
            this.school = null;
            this.classroom = null;
            this.rate = null;
            this.author = null;
            this.isPublished = false;
            this.publishedOn = null;
            this.content = null;
            this.conclusion = null;
            this.lastModifUser = null;
            this.version = null;
            this.goods = [];
            this.bads = [];
            this.tags = [];
            this.ratings = [];
            this.comments = [];
        }
        return (LessonDTO);
    })
    .factory('LessonSummaryDTO', function () {
        function LessonSummaryDTO() {
            this.lessonId = null;
            this.title = null;
        }
        return (LessonSummaryDTO);
    })
    .factory('CommentDTO', function () {
        function CommentDTO() {
            this.lessonId = null;
            this.id = null;
            this.content = null;
            this.date = null;
            this.parentId = null;
            this.level = 0;
            this.author = {
                userid: null,
                username: null,
                image: null
            };
            this.order = 0.0;
            this.status = 'I'; //Initialized
        }
        return (CommentDTO);
    })
    .factory('RatingDTO', function () {
        function RatingDTO() {
            this.id = null;
            this.lessonId = null;
            this.author = {
                userid: null,
                username: null,
                image: null
            };
            this.rating = null;
            this.content = null;
            this.date = null;
            this.version = null;
        }
        return (RatingDTO);
    })
    .factory('LessonService', [
        '$resource',
        '$http',
        '$q',
        'LessonDTO',
        'CommentDTO',
        'RatingDTO',
        'LessonSummaryDTO',
        'DisciturSettings',
        'DiscUtil',
        '$cacheFactory',
        function ($resource, $http, $q, LessonDTO, CommentDTO, RatingDTO, LessonSummaryDTO, DisciturSettings, DiscUtil, $cacheFactory) {
            //-------- private methods -------
            // Private methods for DTO purposes

            // Lesson Data Transfer
            var _dataTransfer = function (lessonData) {
                var lesson = new LessonDTO();
                lesson.lessonId = lessonData.LessonId;
                lesson.title = lessonData.Title;
                lesson.discipline = lessonData.Discipline;
                lesson.school = lessonData.School;
                lesson.classroom = lessonData.Classroom;
                lesson.author = {
                    name: lessonData.Author.Name,
                    surname: lessonData.Author.Surname,
                    userid: lessonData.Author.UserId,
                    username: lessonData.Author.UserName
                }
                lesson.isPublished = lessonData.Published=='1';
                lesson.publishedOn = lessonData.PublishDate;
                lesson.rate = lessonData.Rate;
                angular.forEach(lessonData.FeedBacks, function (feedBack, key) {
                    var fb = { id: feedBack.LessonFeedbackId, content: feedBack.Feedback, status: 'I' };
                    if (feedBack.Nature == 1) this.goods.push(fb)
                    if (feedBack.Nature == 2) this.bads.push(fb)
                    //if (feedBack.Nature == 1) this.goods.push(feedBack.Feedback)
                    //if (feedBack.Nature == 2) this.bads.push(feedBack.Feedback)
                    }, lesson);
                angular.forEach(lessonData.Tags, function (tag, key) {
                    this.tags.push({ content: tag.LessonTagName, status: 'I' });
                }, lesson);
                lesson.tags.status = 'I';
                lesson.content = lessonData.Content;
                lesson.conclusion = lessonData.Conclusion;
                lesson.lastModifUser = lessonData.LastModifUser;
                lesson.version = lessonData.Vers;
                return lesson;
            }
            // Lesson Array Data Transfer
            var _arrayDataTransfer = function (resultArray) {
                var lessons = [];
                for (var i = 0; i < resultArray.length; i++) {
                    lessons.push(_dataTransfer(resultArray[i]));
                }
                return lessons;
            }
            // Paged Lesson Array Data Transfer
            var _pageDataTransfer = function (resultPage) {
                var page = {
                    startRow : resultPage.StartRow,
                    count: resultPage.Count,
                    pageSize: resultPage.PageSize,
                    lessons: _arrayDataTransfer(resultPage.Records)
                }
                return page;
            }
            // Lesson Comment data Transfer
            var _commentTransfer = function (commentData) {
                var comment = new CommentDTO();
                comment.lessonId = commentData.LessonId;
                comment.id = commentData.Id;
                comment.content = commentData.Content;
                comment.date = commentData.Date;
                comment.parentId = commentData.ParentId || 0;
                comment.level = commentData.Level
                comment.author.userid = commentData.Author.UserId;
                comment.author.username = commentData.Author.UserName;
                //comment.author.image = commentData.Author.Picture;
                comment.author.image = commentData.Author.Thumb || commentData.Author.Picture;
                return comment;
            }
            // Lesson Comments array data Transfer
            // The method transfer Comment ApiData and set client Comment poperties (usefuk for sorting):
            // _num: progress number of lesson comment
            // _order: string for lesson comment sorting
            var _commentsArrayTransfer = function (commentArrayData) {
                var comments = [];
                for (var i = 0; i < commentArrayData.length; i++) {
                    comments.push(_commentTransfer(commentArrayData[i]));
                }
                if (comments.length > 0) {
                    comments.sort(function (c1,c2) { return c1.id - c2.id })
                }
                for (var i = 0; i < comments.length; i++) {
                    comments[i]._num = i+1;
                    comments[i]._order = _getCommentOrderString(comments[i], comments);
                }
                return comments;
            }
            // utility method to left padding with "0"
            var lpad = function padDigits(number, digits) {
                return Array(Math.max(digits - String(number).length + 1, 0)).join(0) + number;
            }
            // get client property value _order: the method define the Lesson Comment sorting algorith 
            var _getCommentOrderString = function (comment, commentsArray) {
                var order = "";
                if (comment.level > 0) {
                    for (var i = 0; i < commentsArray.length; i++) {
                        if (comment.parentId == commentsArray[i].id) {
                            order += _getCommentOrderString(commentsArray[i], commentsArray);
                            order += lpad(comment._num, 3);
                        }
                    }
                }
                if (comment.level == 0) {
                    order = "0." + lpad(comment._num, 3) + order;
                }
                return order;
            }
            // Lesson Rating array data Transfer
            var _ratingsArrayTransfer = function (ratingArrayData) {
                var ratings = [];
                for (var i = 0; i < ratingArrayData.length; i++) {
                    ratings.push(_ratingTransfer(ratingArrayData[i]));
                }
                if (ratings.length > 0) {
                    ratings.sort(function (c1, c2) { return c1.date - c2.date })
                }
                return ratings;
            }
            // Lesson Summaries data transfer
            var _lessonSummariesTransfer = function(lessonsData){
                var lsa = []
                for (var i = 0; i < lessonsData.length; i++) {
                    var ls = new  LessonSummaryDTO();
                    ls.lessonId = lessonsData[i].Key;
                    ls.title = lessonsData[i].Value;
                    lsa.push(ls);
                }
                return lsa;
            }
            // Lesson Comment data Transfer
            var _ratingTransfer = function (ratingData) {
                var rating = new RatingDTO();
                rating.id = ratingData.Id;
                rating.lessonId = ratingData.LessonId;
                rating.content = ratingData.Content;
                rating.date = ratingData.CreationDate;
                rating.rating = ratingData.Rating;
                rating.author.userid = ratingData.Author.UserId;
                rating.author.username = ratingData.Author.UserName;
                //rating.author.image = ratingData.Author.Picture;
                rating.author.image = ratingData.Author.Thumb || ratingData.Author.Picture;
                rating.version = ratingData.Vers;
                return rating;
            }
            // lesson mapping
            var _lessonMap = function (lesson) {
                var data2api = {};
                data2api.LessonId = lesson.lessonId;
                data2api.Title = lesson.title;
                data2api.Discipline = lesson.discipline;
                data2api.School = lesson.school;
                data2api.Classroom = lesson.classroom;
                data2api.Rate = null;

                data2api.Author = {
                    Name: lesson.author.name,
                    Surname: lesson.author.surname,
                    UserId: lesson.author.userid
                }
                data2api.Published = lesson.isPublished ? 1 : 0;
                data2api.PublishedOn = lesson.publishedOn;
                data2api.FeedBacks = [];
                angular.forEach(lesson.goods, function (feedBack, key) {
                    var fb = { LessonFeedbackId: feedBack.id, Feedback: feedBack.content, Nature: 1, Status: feedBack.status };
                    this.FeedBacks.push(fb);
                }, data2api);
                angular.forEach(lesson.bads, function (feedBack, key) {
                    var fb = { LessonFeedbackId: feedBack.id, Feedback: feedBack.content, Nature: 2, Status: feedBack.status };
                    this.FeedBacks.push(fb);
                }, data2api);

                data2api.Tags = []
                angular.forEach(lesson.tags, function (tag, key) {
                    var _tag = { LessonId: lesson.lessonId, LessonTagName: tag.content, Status: tag.status };
                    this.Tags.push(_tag);
                }, data2api);

                data2api.Content = lesson.content;
                data2api.Conclusion = lesson.conclusion;
                data2api.LastModifUser = lesson.lastModifUser;
                data2api.Vers = lesson.version;

                if(lesson.status)
                    data2api.Status = lesson.status;
                return data2api;
            }
            // Get Async list of values
            var _getDistinctValues= function (type, inputParams) {
                switch (type) {
                    case('discipline') :
                        DiscUtil.validateInput('LessonService.getDistinctValues.discipline', { disciplineQ: null }, inputParams);
                        break;
                    case ('school'):
                        DiscUtil.validateInput('LessonService.getDistinctValues.school', { schoolQ: null }, inputParams);
                        break;
                    case ('classroom'):
                        DiscUtil.validateInput('LessonService.getDistinctValues.classroom', { classroomQ: null }, inputParams);
                        break;
                    case ('tag'):
                        DiscUtil.validateInput('LessonService.getDistinctValues.tag', { tagQ: null }, inputParams);
                        break;
                    default:
                        throw { code: 20003, message: 'invalid type string for LessonService.getDistinctValues :' + type }
                }

                // create deferring result
                var deferred = $q.defer();

                // Retrieve Async data for lesson id in input        
                $http({ method: 'GET', url: DisciturSettings.apiUrl + 'lesson', params: inputParams, cache: true })
                    .success(
                        // Success Callback: Data Transfer Object Creation
                        function (result) {
                            deferred.resolve(result)
                        })
                    .error(
                        // Error Callback
                        function (data) {
                            deferred.reject("Error for LessonService.getDistinctValues:" + data);
                        });
                // create deferring result
                return deferred.promise;

            }


            //-------- private properties -------
            var _currentInput;
            var _currentPage;

            //-------- public methods-------
            var _lessonService =  {
                // Retrieve Async data for lesson id in input 
                // and return a LessonDTO instance
                get: function (inputParams) {
                    DiscUtil.validateInput(
                        'LessonService.get',   // function name for logging purposes
                        { id: null},              // hashmap to check inputParameters
                        inputParams            // actual input params
                    );
                    // create deferring result
                    var deferred = $q.defer();
                    // Retrieve Async data for lesson id in input        
                    // cache is enabled. Only after modification (Lessonservice.save) the chache is reloaded
                    $http.get(DisciturSettings.apiUrl + 'lesson/' + inputParams.id, {cache: true})
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (data, status, headers, config) {
                                deferred.resolve(_dataTransfer(data));
                            })
                        .error(
                            // Error Callback
                            function (data, status, headers, config) {
                                deferred.reject("no Lesson for id:" + inputParams.id);
                            });

                    return deferred.promise;
                },
                // Search Async data for lesson inputParams
                // and return a and array of LessonDTO instances
                search: function (inputParams) {
                    DiscUtil.validateInput(
                        'LessonService.search',       // function name for logging purposes
                        {                             // hashmap to check inputParameters e set default values
                            keyword: null,
                            inContent: null,
                            discipline: null,
                            school: null,
                            classroom: null,
                            rate: null,
                            tags: null,
                            publishedOn: null,
                            publishedBy: null,
                            startRow: 0,
                            pageSize: 3,
                            orderBy: "PublishDate",
                            orderDir: "DESC"
                        }, 
                        inputParams                   // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();
                    
                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'GET', url: DisciturSettings.apiUrl + 'lesson', params: inputParams })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                // save search input e result Data for future paging
                                _currentInput = inputParams;
                                _currentPage = _pageDataTransfer(result)
                                deferred.resolve(_currentPage)
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error for search:" + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Get Async page of lesson based on pageinput
                // and return a and array of LessonDTO instances
                getPage: function (pageinput) {
                    DiscUtil.validateInput(
                        'LessonService.getPage',    // function name for logging purposes
                        {                           // hashmap to check inputParameters e set default values
                            pageNum: null
                        },
                        pageinput                   // actual input params
                        );

                    _currentInput.startRow = (pageinput.pageNum - 1) * _currentInput.pageSize
                    return _currentInput;
                    //return this.search(_currentInput)
                },
                // Get Async list of unique disciplines by value
                getDisciplines : function (q) {
                    return _getDistinctValues('discipline', { disciplineQ: q });
                },
                // Async list of unique schools by value
                getSchools : function (q) {
                    return _getDistinctValues('school', { schoolQ: q });
                },
                // Async list of unique classrooms by value
                getClassRooms : function (q) {
                    return _getDistinctValues('classroom', { classroomQ: q });
                },
                // Async list of unique tags by value
                getTags: function (q) {
                    return _getDistinctValues('tag', { tagQ: q });
                },
                // Get Async list of lesson's users comments
                getComments: function (inputParams) {
                    DiscUtil.validateInput(
                        'LessonService.getComments',       // function name for logging purposes
                        {                             // hashmap to check inputParameters e set default values
                            id: null
                        },
                        inputParams                   // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'GET', url: DisciturSettings.apiUrl + 'lesson/' + inputParams.id + '/comments', cache: true })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                deferred.resolve(_commentsArrayTransfer(result))
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error for getting comments on lesson id:'+ inputParams.id + ' -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Save Async User Comment
                createComment: function (comment, commentsArray) {
                    DiscUtil.validateInput(
                        'LessonService.createComment',       // function name for logging purposes
                        new CommentDTO(),                  // hashmap to check inputParameters e set default values
                        comment                            // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'POST', url: DisciturSettings.apiUrl + 'lesson/' + comment.lessonId + '/comment', data: comment })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                // if success, clear cache of getComments
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + comment.lessonId + '/comments')

                                var _newComment = _commentTransfer(result);
                                // if lesson comments array is passed, the new comment is enriched with client properties
                                _newComment = _lessonService.setCommentPrivates(_newComment, commentsArray);
                                deferred.resolve(_newComment)
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error saving comment on lesson id:"+ comment.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Save Async User Comment
                updateComment: function (comment) {
                    DiscUtil.validateInput(
                        'LessonService.updateComment',       // function name for logging purposes
                        new CommentDTO(),                  // hashmap to check inputParameters e set default values
                        comment                            // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'PUT', url: DisciturSettings.apiUrl + 'lesson/' + comment.lessonId + '/comment/' + comment.id, data: comment })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result, status) {
                                // I don't understand this...I should go on error callback...
                                //if (status >= 200 && status < 300) {
                                // if success, clear cache of getComments
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + comment.lessonId + '/comments')

                                var _newComment = _commentTransfer(result);
                                _newComment._num = comment._num;
                                _newComment._order = comment._order;
                                deferred.resolve(_newComment)
                                //}
                                //else
                                //    deferred.reject("Error editing comment on lesson id:" + comment.lessonId + " -> " + result);
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error editing comment on lesson id:" + comment.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Delete Async User Comment
                deleteComment: function (comment) {
                    DiscUtil.validateInput(
                        'LessonService.deleteComment',  // function name for logging purposes
                        new CommentDTO(),               // hashmap to check inputParameters e set default values
                        comment                         // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // execute logical delete, updating record state (Api business logic)
                    $http({ method: 'PUT', url: DisciturSettings.apiUrl + 'lesson/' + comment.lessonId + '/comment/' + comment.id + '/delete', data: comment })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result, status) {
                                deferred.resolve(comment);
                                /*
                                // I don't understand this...I should go on error callback...
                                if (status >= 200 && status < 300) {
                                    deferred.resolve(comment);
                                }
                                else
                                    deferred.reject("Error deleting comment on lesson id:" + comment.lessonId + " -> " + arguments.toString());
                                */
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error deleting comment on lesson id:" + comment.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // add local Comment properties, for comments sorting purposes
                setCommentPrivates: function (comment, commentsArray) {
                    DiscUtil.validateInput(
                        'LessonService.setCommentPrivates',  // function name for logging purposes
                        new CommentDTO(),                    // hashmap to check inputParameters e set default values
                        comment                              // actual input params
                        );
                    if (commentsArray && commentsArray.constructor == Array) {
                        comment._num = commentsArray.length + 1;
                        comment._order = _getCommentOrderString(comment, commentsArray);
                    }
                    return comment;
                },
                // Get Async list of lesson's users ratings
                getRatings: function (inputParams) {
                    DiscUtil.validateInput(
                        'LessonService.getRatings',   // function name for logging purposes
                        {                             // hashmap to check inputParameters e set default values
                            id: null
                        },
                        inputParams                   // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'GET', url: DisciturSettings.apiUrl + 'lesson/' + inputParams.id + '/ratings', cache: true })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                deferred.resolve(_ratingsArrayTransfer(result))
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error for getting ratings on lesson id:'+ inputParams.id + ' -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Save Async User Rating
                createRating: function (rating) {
                    DiscUtil.validateInput(
                        'LessonService.createRating',       // function name for logging purposes
                        new RatingDTO(),                  // hashmap to check inputParameters e set default values
                        rating                            // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'POST', url: DisciturSettings.apiUrl + 'lesson/' + rating.lessonId + '/rating', data: rating })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                // if success, clear cache of getRatings
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + rating.lessonId + '/ratings')

                                var _newRating = _ratingTransfer(result);
                                deferred.resolve(_newRating)
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error creating rating on lesson id:" + rating.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Save Async User Rating
                updateRating: function (rating) {
                    DiscUtil.validateInput(
                        'LessonService.updateRating',       // function name for logging purposes
                        new RatingDTO(),                  // hashmap to check inputParameters e set default values
                        rating                            // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'PUT', url: DisciturSettings.apiUrl + 'lesson/' + rating.lessonId + '/rating/' + rating.id, data: rating })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                // if success, clear cache of getRatings and lesson (ratings update lesson average rating)
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + rating.lessonId + '/ratings')
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + rating.lessonId)

                                var _modifiedRating = _ratingTransfer(result);
                                deferred.resolve(_modifiedRating)
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error updating rating on lesson id:" + rating.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Delete Async User Comment
                deleteRating: function (rating) {
                    DiscUtil.validateInput(
                        'LessonService.deleteRating',  // function name for logging purposes
                        new RatingDTO(),               // hashmap to check inputParameters e set default values
                        rating                         // actual input params
                        );
                    // create deferring result
                    var deferred = $q.defer();

                    // execute logical delete, updating record state (Api business logic)
                    $http({ method: 'PUT', url: DisciturSettings.apiUrl + 'lesson/' + rating.lessonId + '/rating/' + rating.id + '/delete', data: rating })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result, status) {
                                // I don't understand this...I should go on error callback...
                                if (status >= 200 && status < 300) {
                                    deferred.resolve(rating);
                                }
                                else
                                    deferred.reject("Error deleting comment on lesson id:" + rating.lessonId + " -> " + arguments.toString());
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error deleting comment on lesson id:" + rating.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Save Lesson
                update: function (lesson) {
                    DiscUtil.validateInput(
                        'LessonService.update',       // function name for logging purposes
                        new LessonDTO(),            // hashmap to check inputParameters e set default values
                        lesson                      // actual input params
                        );
                    // DTO mappint to API
                    var _lesson = _lessonMap(lesson);
                    // create deferring result
                    var deferred = $q.defer();
                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'PUT', url: DisciturSettings.apiUrl + 'lesson/edit/' + _lesson.LessonId, data: _lesson })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                // if success, clear cache 
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson/' + _lesson.LessonId)
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson?lastNum=' + DisciturSettings.lastLessonsNum)
                                deferred.resolve(_dataTransfer(result))
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error updating lesson id:" + _lesson.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;
                },
                // Create new Lesson
                create: function (lesson) {
                    DiscUtil.validateInput(
                        'LessonService.create',       // function name for logging purposes
                        new LessonDTO(),            // hashmap to check inputParameters e set default values
                        lesson                      // actual input params
                        );
                    var _lesson = _lessonMap(lesson);

                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'POST', url: DisciturSettings.apiUrl + 'lesson', data: _lesson })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                $cacheFactory.get('$http').remove(DisciturSettings.apiUrl + 'lesson?lastNum=' + DisciturSettings.lastLessonsNum)
                                deferred.resolve(_dataTransfer(result))
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error creating lesson id:" + _lesson.lessonId + " -> " + data);
                            });
                    // create deferring result
                    return deferred.promise;

                },
                // New LessonDto Factory
                newLesson: function () {
                    return new LessonDTO();
                },
                // get last 5 lessons (summary data)
                getLastLessons: function () {
                    // create deferring result
                    var deferred = $q.defer();

                    // Retrieve Async data for lesson id in input        
                    $http({ method: 'GET', url: DisciturSettings.apiUrl + 'lesson', params: { lastNum: DisciturSettings.lastLessonsNum }, cache: true })
                        .success(
                            // Success Callback: Data Transfer Object Creation
                            function (result) {
                                deferred.resolve(_lessonSummariesTransfer(result));
                            })
                        .error(
                            // Error Callback
                            function (data) {
                                deferred.reject("Error for getLastLessons:" + data);
                            });
                    // create deferring result
                    return deferred.promise;

                }

            };

            return _lessonService;
      }]);