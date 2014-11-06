angular.module('disc.admin')
    .factory('AdminService', [
        '$http',
        '$q',
        'DiscUtil',
        'DisciturSettings',
        'LabelService',
        function ($http, $q, DiscUtil, DisciturSettings, LabelService) {
            var _adminService = {
                migrate: function () {
                    var deferred = $q.defer();

                    $http.post(DisciturSettings.apiUrl + 'Migrate')
                       .success(
                           // Success Callback
                           function (result) {
                               deferred.resolve(result);
                           })
                       .error(
                           // Error Callback
                           function (error, status) {
                               var _authErr = LabelService.apiError(error);
                               deferred.reject(_authErr);
                           });
                    return deferred.promise;
                },
                //clearReadModel: function () {
                //    var deferred = $q.defer();

                //    $http.post(DisciturSettings.apiUrl + 'ClearReadModel')
                //       .success(
                //           // Success Callback
                //           function (result) {
                //               deferred.resolve(result);
                //           })
                //       .error(
                //           // Error Callback
                //           function (error, status) {
                //               var _authErr = LabelService.apiError(error);
                //               deferred.reject(_authErr);
                //           });
                //    return deferred.promise;
                //},
                replayEvents: function () {
                    var deferred = $q.defer();

                    $http.post(DisciturSettings.apiUrl + 'ReplayAllEvents')
                       .success(
                           // Success Callback
                           function (result) {
                               deferred.resolve(result);
                           })
                       .error(
                           // Error Callback
                           function (error, status) {
                               var _authErr = LabelService.apiError(error);
                               deferred.reject(_authErr);
                           });
                    return deferred.promise;
                }

            }

            return _adminService;
        }
    ]);