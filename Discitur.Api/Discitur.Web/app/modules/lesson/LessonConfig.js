﻿angular.module('disc.lesson',
    [
        'disc.settings',
        'disc.user',
        'disc.common',
        'ui.router',
        'ngSanitize',
        'ui.bootstrap',
        'ui.tinymce'
    ])
    .config(
    [
        '$stateProvider',
        '$urlRouterProvider',
        '$uiViewScrollProvider',
        'DisciturSettings',
        function ($stateProvider, $urlRouterProvider, $uiViewScrollProvider, DisciturSettings) {
            // to prevent autoscroll (introduced by angular-ui-router 0.2.8 https://github.com/angular-ui/ui-router/releases/tag/0.2.8)
            // see: https://github.com/angular-ui/ui-router/issues/787
            $uiViewScrollProvider.useAnchorScroll();

            if (!DisciturSettings.isInMaintenance) {

                // provate method to load Lesson data by lessonId passed through $stateParams
                var _getLessonData = function (LessonService, $q, $stateParams, $state, DiscUtil, AuthService) {
                    // create deferring result
                    var deferred = $q.defer();

                    // During routing phase the $routeParams is not injected yet
                    var lessondId = $stateParams.lessonId //$route.current.params.lessonId;

                    // timeout only for test and study purpose (to erase)
                    //$timeout(function () {
                    LessonService.get({ id: lessondId })
                        .then(
                            // Success Callback
                            function (lesson) {
                                //var cache = $cacheFactory('disciturCache');
                                //cache.put('currentLesson', result)
                                // if lesson is private is visible only for the author
                                if (!lesson.isPublished && lesson.author.userid != AuthService.user.userid) {
                                    deferred.reject("no Lesson for id:" + lessondId);
                                }
                                DiscUtil.cache.put('lesson', lesson)
                                deferred.resolve(lesson)
                            },
                            // Error Callback
                            function () {
                                deferred.reject("no Lesson for id:" + lessondId);
                                //$state.go('404lesson')

                            });
                    //}, 2000);

                    return deferred.promise;
                }


                $stateProvider
                    .state('lessonSearch', {
                        url: 'lesson?keyword?discipline?school?classroom?rate?tags?publishedOn?publishedBy?startRow?pageSize?orderBy?orderDir',
                        parent: 'master.2cl',
                        onEnter: function () {
                            console.log("Entering Lesson Search");
                        },
                        resolve: {
                            lessonsData: ['LessonService', '$stateParams', function (LessonService, $stateParams) {
                                return LessonService.search($stateParams);
                            }],
                            lastLessonList: ['LessonService', function (LessonService) {
                                return LessonService.getLastLessons();
                            }]
                        },
                        views: {
                            'sidebar': {
                                templateUrl: 'modules/lesson/LessonListSideBar.html',
                                controller: 'LessonListSideBarCtrl'
                            },
                            'main': {
                                templateUrl: 'modules/lesson/LessonList.html',
                                controller: 'LessonListCtrl'
                            }
                        }
                    })
                    .state('lessonDetail', {
                        url: 'lesson/:lessonId/{title}',
                        parent: 'master.2cl',
                        onEnter: function () {
                            console.log("Entering Lesson Detail");
                        },
                        // resolve create service data shared by component views
                        resolve: {
                            lessonData: ['LessonService', '$q', '$stateParams', '$state', 'DiscUtil', 'AuthService', _getLessonData],
                            lastLessonList: ['LessonService', function (LessonService) {
                                return LessonService.getLastLessons();
                            }]
                        },
                        views: {
                            'sidebar': {
                                templateUrl: 'modules/lesson/LessonSideBar.html',
                                controller: 'LessonSideBarCtrl'
                            },
                            'main': {
                                templateUrl: 'modules/lesson/Lesson.html',
                                controller: 'LessonCtrl'
                            }
                        }
                    })
                    .state('lessonEdit', {
                        authorized: true,
                        url: 'edit/lesson/:lessonId',
                        parent: 'master.1cl',
                        onEnter: ['AuthService', 'lessonData', '$location', function (AuthService, lessonData, $location) {
                            console.log("Entering Lesson Edit");
                            // the controller can be accessed only if authenticated
                            if (!AuthService.user.isLogged ||
                                (lessonData.lessonId != null && lessonData.author.userid != AuthService.user.userid))
                                // use location due to $state.go land on blank page...
                                $location.path('lesson');
                        }],
                        templateUrl: 'modules/lesson/LessonEdit.html',
                        controller: 'LessonEditCtrl',
                        resolve: {
                            lessonData: ['LessonService', '$q', '$stateParams', '$state', 'DiscUtil', 'AuthService', function (LessonService, $q, $stateParams, $state, DiscUtil, AuthService) {
                                // try to get lesson from cache
                                // if not exists then load from service
                                var lessondId = $stateParams.lessonId
                                if (lessondId) {
                                    var cachedLessonData = DiscUtil.cache.get('lesson')

                                    if (!angular.isDefined(cachedLessonData) || cachedLessonData.lessonId.toString() !== lessondId)
                                        return _getLessonData(LessonService, $q, $stateParams, $state, DiscUtil, AuthService);
                                    else
                                        return cachedLessonData;
                                }
                                else
                                    return LessonService.newLesson();
                            }]
                        }

                    })
                    .state('404lesson', {
                        //authorized: true,
                        url: '404lesson',
                        parent: 'master.2cl',
                        onEnter: function () {
                            console.log("master.2cl.404lesson");
                        },
                        resolve: {
                            lastLessonList: ['LessonService', function (LessonService) {
                                return LessonService.getLastLessons();
                            }]
                        },
                        views: {
                            'sidebar': {
                                templateUrl: 'modules/lesson/LessonListSideBar.html',
                                controller: 'LessonListSideBarCtrl'
                            },
                            'main': {
                                controller: 'Lesson404Ctrl',
                                templateUrl: 'modules/lesson/Lesson404.html'
                            }
                        }
                    });
            }

        }

    ]
)